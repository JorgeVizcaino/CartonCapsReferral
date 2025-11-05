using Application.Interfaces;
using Application.Referrals.Services;
using Domain.Dto;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Application.Tests.Referrals;

public sealed class ReferralsServicesTests
{
    [Fact]
    public async Task CreateAsync_PersistsReferralWithExpectedValues()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("referrals-create")
            .Options;

        await using var context = new AppDbContext(options);
        var userId = Guid.NewGuid();
        context.Users.Add(new UserApp
        {
            Id = userId,
            DisplayName = "Casey Coder",
            ReferralCode = "ABC123"
        });
        await context.SaveChangesAsync();

        var fakeFraud = new TestFraudService
        {
            IsBlockedAsyncFunc = _ => Task.FromResult(false),
            WithinCreateBudgetAsyncFunc = _ => Task.FromResult(true)
        };
        var linkServices = new LinkServices();
        var sut = new ReferralsServices(fakeFraud, context, linkServices, fakeFraud);
        var request = new ReferralRequest { UserId = userId, Campaign = "launch-2025" };
        var before = DateTimeOffset.UtcNow;

        // Act
        var referral = await sut.CreateAsync(request, CancellationToken.None);
        var after = DateTimeOffset.UtcNow;

        // Assert
        Assert.NotNull(referral);
        Assert.Equal(userId, referral.ReferrerUserId);
        Assert.Equal("ABC123", referral.ReferralCode);
        Assert.Equal("launch-2025", referral.Campaign);
        Assert.NotNull(referral.Link);
        Assert.NotNull(referral.Token);
        Assert.NotNull(referral.Slug);
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
    public async Task ValidateSlug_ReturnsExpectedResult_WhenSignatureValidAndReferralExists()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("referrals-validate-success")
            .Options;

        await using var context = new AppDbContext(options);
        var referral = Referral.Create(Guid.NewGuid(), "REF-CODE", "spring");
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

        var sut = new ReferralsServices(fakeFraud, context, new LinkServices(), fakeFraud);
        var request = new ReferralValidations
        {
            ReferralCode = referral.ReferralCode,
            Slug = referral.Slug
        };

        // Act
        var (validated, referralId) = await sut.ValidateSlug(request, CancellationToken.None);

        // Assert
        Assert.True(validated);
        Assert.Equal(referral.Token, referralId);
    }

    [Fact]
    public async Task ValidateSlug_ReturnsFalse_WhenSignatureRejected()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("referrals-validate-signature")
            .Options;

        await using var context = new AppDbContext(options);
        var referral = Referral.Create(Guid.NewGuid(), "REF-CODE", null);
        referral.Slug = "slug-12345";
        referral.Token = "token-xyz";
        referral.ExpiresAt = DateTimeOffset.UtcNow.AddDays(1);
        context.Referrals.Add(referral);
        await context.SaveChangesAsync();

        var fakeFraud = new TestFraudService
        {
            ValidateVendorSignatureAsyncFunc = _ => Task.FromResult(false)
        };

        var sut = new ReferralsServices(fakeFraud, context, new LinkServices(), fakeFraud);
        var request = new ReferralValidations
        {
            ReferralCode = referral.ReferralCode,
            Slug = referral.Slug
        };

        // Act
        var (validated, referralId) = await sut.ValidateSlug(request, CancellationToken.None);

        // Assert
        Assert.False(validated);
        Assert.Equal(string.Empty, referralId);
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
