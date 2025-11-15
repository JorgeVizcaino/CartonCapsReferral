using Application.Interfaces;
using Application.Referrals.Services;
using Domain.Dto;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Application.Tests.Referrals;

public sealed class ReferralsServicesTests
{
    [Fact]
    public async Task CreateAsync_PersistsReferralWithExpectedValues()
    {
        var (dbContext, sqliteConnection) = await CreateContextAsync();
        await using var connection = sqliteConnection;
        await using var context = dbContext;

        var userId = Guid.NewGuid();
        context.Users.Add(new UserApp
        {
            Id = userId,
            DisplayName = "Casey Coder",
            ReferralCode = "ABC123"
        });
        await context.SaveChangesAsync();

        var fakeFraud = new TestFraudService();
        var sut = new ReferralsServices(fakeFraud, context, new LinkServices());
        var request = new ReferralRequest { UserId = userId, Campaign = "launch-2025" };
        var before = DateTimeOffset.UtcNow;

        var referral = await sut.CreateAsync(request, CancellationToken.None);
        var after = DateTimeOffset.UtcNow;

        Assert.NotNull(referral);
        Assert.Equal(userId, referral.ReferrerUserId);
        Assert.Equal("ABC123", referral.ReferralCode);
        Assert.Equal("launch-2025", referral.Campaign);
        Assert.NotNull(referral.Link);
        Assert.False(string.IsNullOrWhiteSpace(referral.Token));
        Assert.StartsWith("https://ccappReferrals.link/", referral.Link);
        Assert.Equal(ReferralStatus.Pending, referral.Status);
        Assert.StartsWith(referral.Id.ToString("N"), referral.Token);
        Assert.Equal(10, referral.Slug.Length);

        Assert.NotNull(referral.ExpiresAt);
        Assert.True(referral.ExpiresAt >= before.AddDays(30));
        Assert.True(referral.ExpiresAt <= after.AddDays(30));

        var storedReferral = await context.Referrals.SingleAsync();
        Assert.Equal(referral.Id, storedReferral.Id);
        Assert.Equal(referral.Token, storedReferral.Token);
    }

    [Fact]
    public async Task CreateAsync_ReturnsExistingReferral_WhenIdempotencyKeyMatches()
    {
        var (dbContext, sqliteConnection) = await CreateContextAsync();
        await using var connection = sqliteConnection;
        await using var context = dbContext;

        var userId = Guid.NewGuid();
        context.Users.Add(new UserApp
        {
            Id = userId,
            DisplayName = "Jordan Dev",
            ReferralCode = "XYZ987"
        });
        await context.SaveChangesAsync();

        var sut = new ReferralsServices(new TestFraudService(), context, new LinkServices());
        var request = new ReferralRequest { UserId = userId, Campaign = "beta", IdempotencyKey = "create-123" };

        var first = await sut.CreateAsync(request, CancellationToken.None);
        var second = await sut.CreateAsync(request, CancellationToken.None);

        Assert.Equal(first.Id, second.Id);
        Assert.Equal(1, await context.Referrals.CountAsync());
    }

    [Fact]
    public async Task ValidateSlugAsync_ReservesReferral_WhenSignatureValid()
    {
        var (dbContext, sqliteConnection) = await CreateContextAsync();
        await using var connection = sqliteConnection;
        await using var context = dbContext;

        var referral = Referral.Create(Guid.NewGuid(), "REF-CODE", "spring", null);
        referral.Slug = "slug-12345";
        referral.Token = "token-xyz";
        referral.Link = $"https://ccappReferrals.link/{referral.Slug}";
        referral.ExpiresAt = DateTimeOffset.UtcNow.AddDays(1);

        context.Referrals.Add(referral);
        await context.SaveChangesAsync();

        var fakeFraud = new TestFraudService
        {
            ValidateVendorSignatureAsyncFunc = _ => Task.FromResult(true)
        };

        var sut = new ReferralsServices(fakeFraud, context, new LinkServices());
        var request = new ReferralValidations
        {
            ReferralCode = referral.ReferralCode,
            Slug = referral.Slug,
            VendorSignature = "valid-sig"
        };

        var result = await sut.ValidateSlugAsync(request, CancellationToken.None);

        Assert.True(result.Validated);
        Assert.Equal(referral.Token, result.Token);

        var updated = await context.Referrals.SingleAsync();
        Assert.Equal(ReferralStatus.Reserved, updated.Status);
        Assert.NotNull(updated.ReservedAtUtc);
    }

    [Fact]
    public async Task ValidateSlugAsync_Throws_WhenSignatureRejected()
    {
        var (dbContext, sqliteConnection) = await CreateContextAsync();
        await using var connection = sqliteConnection;
        await using var context = dbContext;

        var referral = Referral.Create(Guid.NewGuid(), "REF-CODE", null, null);
        referral.Slug = "slug-12345";
        referral.Token = "token-xyz";
        referral.ExpiresAt = DateTimeOffset.UtcNow.AddDays(1);
        context.Referrals.Add(referral);
        await context.SaveChangesAsync();

        var fakeFraud = new TestFraudService
        {
            ValidateVendorSignatureAsyncFunc = _ => Task.FromResult(false)
        };

        var sut = new ReferralsServices(fakeFraud, context, new LinkServices());
        var request = new ReferralValidations
        {
            ReferralCode = referral.ReferralCode,
            Slug = referral.Slug,
            VendorSignature = "short"
        };

        await Assert.ThrowsAsync<ReferralValidationException>(() => sut.ValidateSlugAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task AcceptReferralAsync_Throws_WhenExpired()
    {
        var (dbContext, sqliteConnection) = await CreateContextAsync();
        await using var connection = sqliteConnection;
        await using var context = dbContext;

        var referral = Referral.Create(Guid.NewGuid(), "REF", null, null);
        referral.Slug = "slug";
        referral.Token = "token";
        referral.ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-5);

        context.Referrals.Add(referral);
        await context.SaveChangesAsync();

        var sut = new ReferralsServices(new TestFraudService(), context, new LinkServices());

        await Assert.ThrowsAsync<ReferralExpiredException>(() => sut.AcceptReferralAsync("token", CancellationToken.None));
    }

    private static async Task<(AppDbContext Context, SqliteConnection Connection)> CreateContextAsync()
    {
        var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();
        return (context, connection);
    }

    private sealed class TestFraudService : IFraudService
    {
        public Func<Guid, Task<bool>> IsBlockedAsyncFunc { get; set; } = _ => Task.FromResult(false);
        public Func<Guid, Task<bool>> WithinCreateBudgetAsyncFunc { get; set; } = _ => Task.FromResult(true);
        public Func<string?, Task<bool>> ValidateVendorSignatureAsyncFunc { get; set; } = _ => Task.FromResult(true);

        public Task<bool> IsBlockedAsync(Guid userId) => IsBlockedAsyncFunc(userId);

        public Task<bool> WithinCreateBudgetAsync(Guid userId) => WithinCreateBudgetAsyncFunc(userId);

        public Task<bool> ValidateVendorSignatureAsync(string? signature) => ValidateVendorSignatureAsyncFunc(signature);
    }
}
