using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.Core.Application.Persistence;

using PANiXiDA.TacticalHeroes.Builds.Domain.Builds.Abstractions;
using PANiXiDA.TacticalHeroes.Builds.Domain.Builds.ValueObjects;
using PANiXiDA.TacticalHeroes.Builds.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Builds;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Enumerations;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.ValueObjects;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Players;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.MatchSearches.Read.DbModels;
using PANiXiDA.TacticalHeroes.Matchmaking.Presentation.Features.MatchSearches.Start;
using PANiXiDA.TacticalHeroes.Players.Domain.Players.Abstractions;
using PANiXiDA.TacticalHeroes.Players.Domain.Users;
using PANiXiDA.TacticalHeroes.Players.Infrastructure.Persistence.Core;

using Build = PANiXiDA.TacticalHeroes.Builds.Domain.Builds.Build;
using BuildPlayerId = PANiXiDA.TacticalHeroes.Builds.Domain.Players.PlayerId;
using Player = PANiXiDA.TacticalHeroes.Players.Domain.Players.Player;
using PlayerRating = PANiXiDA.TacticalHeroes.Players.Domain.Players.ValueObjects.Rating;

namespace PANiXiDA.TacticalHeroes.Matchmaking.FunctionalTests.Presentation.Features.MatchSearches.Start;

[Collection(FunctionalTestCollectionDefinition.Name)]
public sealed class StartMatchSearchEndpointTests(FunctionalTestFixture fixture)
{
    private const string Path = "/api/v1/match-searches";

    private static readonly string[] s_modes = ["RankedDuel", "CasualDuel"];

    [Theory(DisplayName = "Endpoint should create search using server data when request is valid")]
    [InlineData("RankedDuel")]
    [InlineData("CasualDuel")]
    public async Task Post_Should_CreateSearchUsingServerData_When_RequestIsValid(string mode)
    {
        var ct = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(ct);
        var player = await SeedPlayerAsync(ct);
        var userId = player.UserId.Value;
        var buildId = (await SeedBuildAsync(player.Id.Value, deleted: false, ct)).Id.Value;
        var before = DateTimeOffset.UtcNow;

        using var response = await PostAsync(
            userId,
            new { buildId, mode, playerId = Guid.CreateVersion7(), rating = 9999 },
            ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await response.Content.ReadAsStringAsync(ct)).ShouldBeEmpty();
        await using var scope = fixture.Services.CreateAsyncScope();
        var row = await scope.ServiceProvider.GetRequiredService<MatchmakingReadDbContext>()
            .Set<MatchSearchReadDbModel>()
            .SingleAsync(x => x.PlayerId == player.Id.Value, ct);
        row.Status.ShouldBe("Searching");
        row.QueuedAt.ShouldBeGreaterThanOrEqualTo(before);
        row.ExpiresAt.ShouldBe(row.QueuedAt.AddMinutes(5));
        row.PlayerId.ShouldBe(player.Id.Value);
        row.BuildId.ShouldBe(buildId);
        row.Rating.ShouldBe(1742);
        row.Mode.ShouldBe(mode);
        row.GameId.ShouldBeNull();
    }

    [Fact(DisplayName = "Endpoint should reject request when user is unavailable")]
    public async Task Post_Should_RejectRequest_When_UserIsUnavailable()
    {
        using var response = await PostAsync(
            null,
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
        var player = await SeedPlayerAsync(ct);
        var search = await SeedSearchAsync(player.Id.Value, matched, overdue, ct);
        var build = await SeedBuildAsync(player.Id.Value, deleted: false, ct);

        using var response = await PostAsync(
            player.UserId.Value,
            new StartMatchSearchRequest(build.Id.Value, "CasualDuel"),
            ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict, await response.Content.ReadAsStringAsync(ct));
        await AssertSearchCountAsync(1, ct);
        await using var scope = fixture.Services.CreateAsyncScope();
        var restored = (await scope.ServiceProvider.GetRequiredService<IMatchSearchesRepository>()
            .GetByIdAsync(search.Id, ct))!;
        restored.Status.ShouldBe(matched ? MatchSearchStatus.Matched : MatchSearchStatus.Searching);
    }

    [Fact(DisplayName = "Endpoint should reject another search when search deadline passed")]
    public async Task Post_Should_RejectAnotherSearch_When_SearchDeadlinePassed()
    {
        var ct = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(ct);
        var player = await SeedPlayerAsync(ct);
        var previous = await SeedSearchAsync(player.Id.Value, matched: false, overdue: true, ct);
        var build = await SeedBuildAsync(player.Id.Value, deleted: false, ct);

        using var response = await PostAsync(
            player.UserId.Value,
            new StartMatchSearchRequest(build.Id.Value, "RankedDuel"),
            ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        await AssertSearchCountAsync(1, ct);
        await using var scope = fixture.Services.CreateAsyncScope();
        var restored = (await scope.ServiceProvider.GetRequiredService<IMatchSearchesRepository>()
            .GetByIdAsync(previous.Id, ct))!;
        restored.Status.ShouldBe(MatchSearchStatus.Searching);
    }

    [Fact(DisplayName = "Endpoint should create one search when requests arrive concurrently")]
    public async Task Post_Should_CreateOneSearch_When_RequestsArriveConcurrently()
    {
        var ct = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(ct);
        var player = await SeedPlayerAsync(ct);
        var buildId = (await SeedBuildAsync(player.Id.Value, deleted: false, ct)).Id.Value;
        var requests = s_modes.Select(mode =>
            PostAsync(player.UserId.Value, new StartMatchSearchRequest(buildId, mode), ct));

        var responses = await Task.WhenAll(requests);

        try
        {
            responses.Count(response => response.StatusCode == HttpStatusCode.NoContent).ShouldBe(1);
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

    [Fact(DisplayName = "Endpoint should reject request when player is missing")]
    public async Task Post_Should_RejectRequest_When_PlayerIsMissing()
    {
        var ct = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(ct);

        using var response = await PostAsync(
            Guid.CreateVersion7(), new StartMatchSearchRequest(Guid.CreateVersion7(), "RankedDuel"), ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        await AssertSearchCountAsync(0, ct);
    }

    [Theory(DisplayName = "Endpoint should reject request when build is unavailable")]
    [InlineData("Missing")]
    [InlineData("Foreign")]
    [InlineData("Deleted")]
    public async Task Post_Should_RejectRequest_When_BuildIsUnavailable(string scenario)
    {
        var ct = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(ct);
        var player = await SeedPlayerAsync(ct);
        var buildId = scenario == "Missing"
            ? Guid.CreateVersion7()
            : (await SeedBuildAsync(
                scenario == "Foreign" ? Guid.CreateVersion7() : player.Id.Value,
                deleted: scenario == "Deleted",
                ct)).Id.Value;

        using var response = await PostAsync(
            player.UserId.Value, new StartMatchSearchRequest(buildId, "RankedDuel"), ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        await AssertSearchCountAsync(0, ct);
    }

    private async Task<Build> SeedBuildAsync(Guid playerId, bool deleted, CancellationToken ct)
    {
        var build = Build.Create(BuildPlayerId.Create(playerId).Value, BuildName.Create("Matchmaking build").Value);
        await using var scope = fixture.Services.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBuildsRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(BuildsWriteDbContext));
        repository.Add(build);
        await unitOfWork.SaveChangesAsync(ct);
        if (deleted)
        {
            repository.Delete(build);
            await unitOfWork.SaveChangesAsync(ct);
        }

        return build;
    }

    private async Task<Player> SeedPlayerAsync(CancellationToken ct)
    {
        var player = Player.Create(UserId.New());
        player.ChangeRating(PlayerRating.Create(1742));
        await using var scope = fixture.Services.CreateAsyncScope();
        scope.ServiceProvider.GetRequiredService<IPlayersRepository>().Add(player);
        await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(PlayersWriteDbContext))
            .SaveChangesAsync(ct);

        return player;
    }

    private async Task<MatchSearch> SeedSearchAsync(
        Guid playerId, bool matched, bool overdue, CancellationToken ct)
    {
        var queuedAt = DateTimeOffset.UtcNow.AddMinutes(overdue ? -6 : 0);
        var search = MatchSearch.Create(
            PlayerId.Create(playerId).Value,
            BuildId.New(),
            Rating.Create(1000),
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
