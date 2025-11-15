using api.Contracts;
using Domain.Dto;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit.Abstractions;

namespace Application.Tests.Api;

public sealed class ReferralsApiTests : IClassFixture<ReferralsApiFactory>, IAsyncLifetime
{
    private readonly ReferralsApiFactory factory;
    private readonly HttpClient client;
    private readonly ITestOutputHelper output;

    public ReferralsApiTests(ReferralsApiFactory factory, ITestOutputHelper output)
    {
        this.factory = factory;
        client = factory.CreateClient();
        this.output = output;
    }

    public async Task InitializeAsync() => await factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CreateReferral_ReturnsCreatedReferral()
    {
        var request = new ReferralRequest { Campaign = "integration" };

        var response = await client.PostAsJsonAsync("/api/referrals", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<ReferralResponse>();
        Assert.NotNull(created);
        Assert.Equal("integration", created!.Campaign);
        Assert.Equal("Pending", created.Status.ToString());

        var listResponse = await client.GetAsync("/api/referrals");
        var listPayload = await listResponse.Content.ReadAsStringAsync();
        output.WriteLine($"GET /api/referrals => {listResponse.StatusCode} {listPayload}");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var referrals = JsonSerializer.Deserialize<List<ReferralResponse>>(listPayload, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        Assert.NotNull(referrals);
        Assert.Single(referrals!);
        Assert.Equal(created.Id, referrals!.Single().Id);
    }

    [Fact]
    public async Task AcceptReferral_ReturnsProblemDetails_WhenTokenUnknown()
    {
        var response = await client.PostAsync("/api/referrals/unknown-token/accept", null);
        var body = await response.Content.ReadAsStringAsync();
        output.WriteLine($"POST /api/referrals/unknown-token/accept => {response.StatusCode} {body}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = JsonSerializer.Deserialize<ProblemDetails>(body, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        Assert.NotNull(problem);
        Assert.Equal("referral.not_found", problem!.Type);
    }

    [Fact]
    public async Task GraphQL_ReturnsReferralStats()
    {
        var createResponse = await client.PostAsJsonAsync("/api/referrals", new ReferralRequest { Campaign = "graph" });
        var created = await createResponse.Content.ReadFromJsonAsync<ReferralResponse>();
        Assert.NotNull(created);

        var payload = new
        {
            query = "query ($userId: UUID!) { referralStats(userId: $userId) { total pending } }",
            variables = new { userId = created!.ReferrerUserId }
        };

        var gqlResponse = await client.PostAsJsonAsync("/graphql", payload);
        var gqlBody = await gqlResponse.Content.ReadAsStringAsync();
        output.WriteLine($"POST /graphql => {gqlResponse.StatusCode} {gqlBody}");
        gqlResponse.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(gqlBody);
        var root = document.RootElement.GetProperty("data").GetProperty("referralStats");
        Assert.True(root.GetProperty("total").GetInt32() >= 1);
        Assert.True(root.GetProperty("pending").GetInt32() >= 1);
    }
}
