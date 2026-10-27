using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.Core.Application.Persistence;

using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Builds;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Enumerations;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.ValueObjects;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Players;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.MatchSearches.Read.DbModels;
using PANiXiDA.TacticalHeroes.Matchmaking.Presentation.Features.MatchSearches.Start;

namespace PANiXiDA.TacticalHeroes.Matchmaking.FunctionalTests.Presentation.Features.MatchSearches.Start;

[Collection(FunctionalTestCollectionDefinition.Name)]
public sealed class StartMatchSearchEndpointTests(FunctionalTestFixture fixture)
{
    private const string Path = "/api/v1/match-searches";

    [Theory(DisplayName = "Endpoint should create search using server data when request is valid")]
    [InlineData("RankedDuel")]
    [InlineData("CasualDuel")]
    public async Task Post_Should_CreateSearchUsingServerData_When_RequestIsValid(string mode)
    {
        var ct = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(ct);
        var userId = Guid.CreateVersion7();
        var buildId = Guid.CreateVersion7();
        var before = DateTimeOffset.UtcNow;

        using var response = await PostAsync(
            userId,
            new { buildId, mode, playerId = Guid.CreateVersion7(), rating = 9999 },
            ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Created, await response.Content.ReadAsStringAsync(ct));
        var result = (await response.Content.ReadFromJsonAsync<StartMatchSearchResponse>(ct))!;
        result.Status.ShouldBe("Searching");
        result.QueuedAt.ShouldBeGreaterThanOrEqualTo(before);
        result.ExpiresAt.ShouldBe(result.QueuedAt.AddMinutes(5));
        await using var scope = fixture.Services.CreateAsyncScope();
        var row = await scope.ServiceProvider.GetRequiredService<MatchmakingReadDbContext>()
            .Set<MatchSearchReadDbModel>()
            .SingleAsync(x => x.Id == result.Id, ct);
        row.PlayerId.ShouldBe(userId);
        row.BuildId.ShouldBe(buildId);
        row.Rating.ShouldBe(1000);
        row.Mode.ShouldBe(mode);
        row.GameId.ShouldBeNull();
    }

    [Theory(DisplayName = "Endpoint should reject request when user is unavailable")]
    [InlineData(null)]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task Post_Should_RejectRequest_When_UserIsUnavailable(string? value)
    {
        using var response = await PostAsync(
            value is null ? null : Guid.Parse(value),
            new StartMatchSearchRequest(Guid.CreateVersion7(), "RankedDuel"),
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Theory(DisplayName = "Endpoint should reject request when selection is invalid")]
    [InlineData(true, "RankedDuel")]
    [InlineData(false, "Unknown")]
    [InlineData(false, "")]
    [InlineData(false, null)]
    public async Task Post_Should_RejectRequest_When_SelectionIsInvalid(bool emptyBuildId, string? mode)
    {
        var ct = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(ct);

        using var response = await PostAsync(
            Guid.CreateVersion7(),
            new { buildId = emptyBuildId ? Guid.Empty : Guid.CreateVersion7(), mode },
            ct);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        await AssertSearchCountAsync(0, ct);
    }

    [Theory(DisplayName = "Endpoint should reject another search when player is occupied")]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Post_Should_RejectAnotherSearch_When_PlayerIsOccupied(bool matched, bool overdue)
    {
        var ct = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(ct);
        var playerId = Guid.CreateVersion7();
        var search = await SeedSearchAsync(playerId, matched, overdue, ct);

        using var response = await PostAsync(
            playerId,
            new StartMatchSearchRequest(Guid.CreateVersion7(), "CasualDuel"),
            ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict, await response.Content.ReadAsStringAsync(ct));
        await AssertSearchCountAsync(1, ct);
        await using var scope = fixture.Services.CreateAsyncScope();
        var restored = (await scope.ServiceProvider.GetRequiredService<IMatchSearchesRepository>()
            .GetByIdAsync(search.Id, ct))!;
        restored.Status.ShouldBe(matched ? MatchSearchStatus.Matched : MatchSearchStatus.Searching);
    }

    [Fact(DisplayName = "Endpoint should expire previous search when search deadline passed")]
    public async Task Post_Should_ExpirePreviousSearch_When_SearchDeadlinePassed()
    {
        var ct = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(ct);
        var playerId = Guid.CreateVersion7();
        var previous = await SeedSearchAsync(playerId, matched: false, overdue: true, ct);

        using var response = await PostAsync(
            playerId,
            new StartMatchSearchRequest(Guid.CreateVersion7(), "RankedDuel"),
            ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Created, await response.Content.ReadAsStringAsync(ct));
        await AssertSearchCountAsync(2, ct);
        await using var scope = fixture.Services.CreateAsyncScope();
        var restored = (await scope.ServiceProvider.GetRequiredService<IMatchSearchesRepository>()
            .GetByIdAsync(previous.Id, ct))!;
        restored.Status.ShouldBe(MatchSearchStatus.Expired);
    }

    [Fact(DisplayName = "Endpoint should create one search when requests arrive concurrently")]
    public async Task Post_Should_CreateOneSearch_When_RequestsArriveConcurrently()
    {
        var ct = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(ct);
        var playerId = Guid.CreateVersion7();
        var buildId = Guid.CreateVersion7();
        var requests = new[] { "RankedDuel", "CasualDuel" }.Select(mode =>
            PostAsync(playerId, new StartMatchSearchRequest(buildId, mode), ct));

        var responses = await Task.WhenAll(requests);

        try
        {
            responses.Count(response => response.StatusCode == HttpStatusCode.Created).ShouldBe(1);
            responses.Count(response => response.StatusCode == HttpStatusCode.Conflict).ShouldBe(1);
            await AssertSearchCountAsync(1, ct);
        }
        finally
        {
            foreach (var response in responses)
            {
                response.Dispose();
            }
        }
    }

    private async Task<MatchSearch> SeedSearchAsync(
        Guid playerId, bool matched, bool overdue, CancellationToken ct)
    {
        var queuedAt = DateTimeOffset.UtcNow.AddMinutes(overdue ? -6 : 0);
        var search = MatchSearch.Create(
            PlayerId.Create(playerId).Value,
            BuildId.New(),
            Raiting.Create(1000),
            MatchmakingMode.RankedDuel,
            MatchmakingTimestamp.Create(queuedAt)).Value;
        if (matched)
        {
            search.Matched(MatchmakingTimestamp.Create(queuedAt)).IsSuccess.ShouldBeTrue();
        }

        await using var scope = fixture.Services.CreateAsyncScope();
        scope.ServiceProvider.GetRequiredService<IMatchSearchesRepository>().Add(search);
        await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(MatchmakingWriteDbContext))
            .SaveChangesAsync(ct);

        return search;
    }

    private async Task<HttpResponseMessage> PostAsync(Guid? userId, object request, CancellationToken ct)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, Path);
        message.Content = JsonContent.Create(request, options: TestJsonSerializerOptions.Web);
        if (userId is { } id)
        {
            message.Headers.Add("X-Test-UserId", id.ToString());
        }

        return await fixture.Client.SendAsync(message, ct);
    }

    private async Task AssertSearchCountAsync(int expected, CancellationToken ct)
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var count = await scope.ServiceProvider.GetRequiredService<MatchmakingReadDbContext>()
            .Set<MatchSearchReadDbModel>()
            .CountAsync(ct);
        count.ShouldBe(expected);
    }
}
