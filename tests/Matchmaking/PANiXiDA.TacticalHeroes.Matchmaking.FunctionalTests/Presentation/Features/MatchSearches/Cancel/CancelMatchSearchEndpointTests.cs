using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.Core.Application.Messaging.Mediator;
using PANiXiDA.Core.Application.Persistence;
using PANiXiDA.Core.ResultPattern;

using PANiXiDA.TacticalHeroes.Builds.Domain.Builds.Abstractions;
using PANiXiDA.TacticalHeroes.Builds.Domain.Builds.ValueObjects;
using PANiXiDA.TacticalHeroes.Builds.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.ConfirmMatch;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Builds;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Games;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Enumerations;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.ValueObjects;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Players;
using PANiXiDA.TacticalHeroes.Matchmaking.FunctionalTests.Presentation.Matching;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.MatchSearches.Read.DbModels;
using PANiXiDA.TacticalHeroes.Matchmaking.Presentation.Features.MatchSearches.Start;
using PANiXiDA.TacticalHeroes.Players.Domain.Players.Abstractions;
using PANiXiDA.TacticalHeroes.Players.Domain.Users;
using PANiXiDA.TacticalHeroes.Players.Infrastructure.Persistence.Core;

using static PANiXiDA.TacticalHeroes.Matchmaking.FunctionalTests.Presentation.Matching.MatchmakingTestSupport;

using Build = PANiXiDA.TacticalHeroes.Builds.Domain.Builds.Build;
using BuildPlayerId = PANiXiDA.TacticalHeroes.Builds.Domain.Players.PlayerId;
using Player = PANiXiDA.TacticalHeroes.Players.Domain.Players.Player;

namespace PANiXiDA.TacticalHeroes.Matchmaking.FunctionalTests.Presentation.Features.MatchSearches.Cancel;

[Collection(FunctionalTestCollectionDefinition.Name)]
public sealed class CancelMatchSearchEndpointTests(FunctionalTestFixture fixture)
{
    private const string Path = "/api/v1/match-searches";

    [Theory(DisplayName = "Endpoint should cancel own search when search is still searching")]
    [InlineData("RankedDuel", false)]
    [InlineData("RankedDuel", true)]
    [InlineData("CasualDuel", false)]
    [InlineData("CasualDuel", true)]
    public async Task Delete_Should_CancelOwnSearch_When_SearchIsStillSearching(string mode, bool overdue)
    {
        var ct = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(ct);
        var player = await SeedPlayerAsync(ct);
        var search = await SeedSearchAsync(
            playerId: player.Id.Value,
            mode: mode,
            status: "Searching",
            overdue: overdue,
            hasGame: false,
            ct: ct);

        using var response = await DeleteAsync(player.UserId.Value, search.Id.Value, ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await response.Content.ReadAsStringAsync(ct)).ShouldBeEmpty();
        var stored = await ReadSearchAsync(search.Id.Value, ct);
        stored.Status.ShouldBe("Cancelled");
        stored.PlayerId.ShouldBe(player.Id.Value);
        stored.Mode.ShouldBe(mode);
        stored.GameId.ShouldBeNull();
    }

    [Fact(DisplayName = "Endpoint should succeed when cancellation is repeated")]
    public async Task Delete_Should_Succeed_When_CancellationIsRepeated()
    {
        var ct = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(ct);
        var player = await SeedPlayerAsync(ct);
        var search = await SeedSearchAsync(player.Id.Value, ct);

        using var initial = await DeleteAsync(player.UserId.Value, search.Id.Value, ct);
        using var repeated = await DeleteAsync(player.UserId.Value, search.Id.Value, ct);

        initial.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        repeated.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await ReadSearchAsync(search.Id.Value, ct)).Status.ShouldBe("Cancelled");
    }

    [Fact(DisplayName = "Endpoint should reject request when user is unauthenticated")]
    public async Task Delete_Should_RejectRequest_When_UserIsUnauthenticated()
    {
        using var response = await DeleteAsync(null, Guid.CreateVersion7(), TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "Endpoint should reject request when search id is empty")]
    public async Task Delete_Should_RejectRequest_When_SearchIdIsEmpty()
    {
        using var response = await DeleteAsync(
            Guid.CreateVersion7(), Guid.Empty, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Theory(DisplayName = "Endpoint should hide search when search is unavailable to user")]
    [InlineData("MissingSearch")]
    [InlineData("Foreign")]
    [InlineData("Deleted")]
    [InlineData("MissingPlayer")]
    public async Task Delete_Should_HideSearch_When_SearchIsUnavailableToUser(string scenario)
    {
        var ct = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(ct);
        var owner = await SeedPlayerAsync(ct);
        var search = await SeedSearchAsync(owner.Id.Value, ct);
        var userId = scenario switch
        {
            "Foreign" => (await SeedPlayerAsync(ct)).UserId.Value,
            "MissingPlayer" => Guid.CreateVersion7(),
            _ => owner.UserId.Value
        };
        var searchId = scenario == "MissingSearch" ? Guid.CreateVersion7() : search.Id.Value;
        if (scenario == "Deleted")
        {
            await using var scope = fixture.Services.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<IMatchSearchesRepository>();
            var restored = (await repository.GetByIdAsync(search.Id, ct))!;
            repository.Delete(restored);
            await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(MatchmakingWriteDbContext))
                .SaveChangesAsync(ct);
        }

        using var response = await DeleteAsync(userId, searchId, ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await ReadSearchAsync(search.Id.Value, ct)).Status.ShouldBe("Searching");
    }

    [Theory(DisplayName = "Endpoint should reject cancellation when search is no longer cancellable")]
    [InlineData("Matched", false)]
    [InlineData("Matched", true)]
    [InlineData("Closed", false)]
    [InlineData("Expired", false)]
    public async Task Delete_Should_RejectCancellation_When_SearchIsNoLongerCancellable(string status, bool hasGame)
    {
        var ct = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(ct);
        var player = await SeedPlayerAsync(ct);
        var search = await SeedSearchAsync(
            playerId: player.Id.Value,
            mode: "RankedDuel",
            status: status,
            overdue: false,
            hasGame: hasGame,
            ct: ct);

        using var response = await DeleteAsync(player.UserId.Value, search.Id.Value, ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        var stored = await ReadSearchAsync(search.Id.Value, ct);
        stored.Status.ShouldBe(status);
        stored.GameId.ShouldBe(search.GameId?.Value);
    }

    [Fact(DisplayName = "Endpoint should preserve new search when cancellation is retried after restart")]
    public async Task Delete_Should_PreserveNewSearch_When_CancellationIsRetriedAfterRestart()
    {
        var ct = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(ct);
        var player = await SeedPlayerAsync(ct);
        var previous = await SeedSearchAsync(player.Id.Value, ct);
        var buildId = await SeedBuildAsync(player.Id.Value, ct);

        using var initial = await DeleteAsync(player.UserId.Value, previous.Id.Value, ct);
        using var request = new HttpRequestMessage(HttpMethod.Post, Path);
        request.Headers.Add("X-Test-UserId", player.UserId.Value.ToString());
        request.Content = JsonContent.Create(
            new StartMatchSearchRequest(buildId, "CasualDuel"), options: TestJsonSerializerOptions.Web);
        using var restarted = await fixture.Client.SendAsync(request, ct);
        using var repeated = await DeleteAsync(player.UserId.Value, previous.Id.Value, ct);

        initial.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        restarted.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        repeated.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        await using var scope = fixture.Services.CreateAsyncScope();
        var rows = await scope.ServiceProvider.GetRequiredService<MatchmakingReadDbContext>()
            .Set<MatchSearchReadDbModel>()
            .Where(search => search.PlayerId == player.Id.Value)
            .ToListAsync(ct);
        rows.Count.ShouldBe(2);
        rows.Single(search => search.Id == previous.Id.Value).Status.ShouldBe("Cancelled");
        rows.Single(search => search.Id != previous.Id.Value).Status.ShouldBe("Searching");
    }

    [Theory(DisplayName = "Endpoint should resolve concurrency when cancellation races with confirmation")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Delete_Should_ResolveConcurrency_When_CancellationRacesWithConfirmation(bool matchWins)
    {
        var ct = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(ct);
        var player = await SeedPlayerAsync(ct);
        var search = await SeedSearchAsync(player.Id.Value, ct);
        var opponent = await SeedSearchAsync(Guid.CreateVersion7(), ct);
        var barrier = fixture.Services.GetRequiredService<ConcurrentMatchSaveBarrier>();
        var reached = barrier.PauseNextSave(matchWins ? MatchSearchStatus.Cancelled : MatchSearchStatus.Matched);
        var command = new ConfirmMatchCommand(search.Id.Value, opponent.Id.Value);

        Task<HttpResponseMessage> cancellation;
        Task<Result> confirmation;
        if (matchWins)
        {
            cancellation = DeleteAsync(player.UserId.Value, search.Id.Value, ct);
            try
            {
                await reached.WaitAsync(TimeSpan.FromSeconds(15), ct);
                confirmation = ConfirmAsync(command, ct);
                await confirmation;
            }
            finally
            {
                barrier.ResumeMatchSave();
            }
        }
        else
        {
            confirmation = ConfirmAsync(command, ct);
            try
            {
                await reached.WaitAsync(TimeSpan.FromSeconds(15), ct);
                cancellation = DeleteAsync(player.UserId.Value, search.Id.Value, ct);
                await cancellation;
            }
            finally
            {
                barrier.ResumeMatchSave();
            }
        }
        using var response = await cancellation;
        var confirmed = await confirmation;
        var rows = await ReadAsync(fixture.Services, [search.Id.Value, opponent.Id.Value], ct);

        rows.Count.ShouldBe(2);
        if (matchWins)
        {
            response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
            confirmed.IsSuccess.ShouldBeTrue();
            rows.ShouldAllBe(row => row.Status == "Matched");
            var found = await WaitForFoundAsync(fixture.Services, search.Id.Value, ct);
            found.SecondMatchSearchId.ShouldBe(opponent.Id.Value);
        }
        else
        {
            response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
            confirmed.Errors.ShouldHaveSingleItem().Type.ShouldBe(ErrorType.Conflict);
            rows.Single(row => row.Id == search.Id.Value).Status.ShouldBe("Cancelled");
            rows.Single(row => row.Id == opponent.Id.Value).Status.ShouldBe("Searching");
            fixture.Services.GetRequiredService<MatchFoundProbe>().Events.ShouldNotContain(
                found => found.FirstMatchSearchId == search.Id.Value || found.SecondMatchSearchId == search.Id.Value);
        }
    }

    private async Task<Player> SeedPlayerAsync(CancellationToken ct)
    {
        var player = Player.Create(UserId.New());
        await using var scope = fixture.Services.CreateAsyncScope();
        scope.ServiceProvider.GetRequiredService<IPlayersRepository>().Add(player);
        await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(PlayersWriteDbContext))
            .SaveChangesAsync(ct);

        return player;
    }

    private Task<MatchSearch> SeedSearchAsync(Guid playerId, CancellationToken ct)
    {
        return SeedSearchAsync(
            playerId: playerId,
            mode: "RankedDuel",
            status: "Searching",
            overdue: false,
            hasGame: false,
            ct: ct);
    }

    private async Task<MatchSearch> SeedSearchAsync(
        Guid playerId,
        string mode,
        string status,
        bool overdue,
        bool hasGame,
        CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var queuedAt = overdue || status == "Expired" ? now.AddMinutes(-6) : now.AddSeconds(-30);
        var search = MatchSearch.Create(
            PlayerId.Create(playerId).Value,
            BuildId.New(),
            Rating.Create(1500),
            MatchmakingMode.Create(mode).Value,
            MatchmakingTimestamp.Create(queuedAt)).Value;
        if (status is "Matched" or "Closed")
        {
            search.Matched(MatchmakingTimestamp.Create(now)).IsSuccess.ShouldBeTrue();
            if (hasGame)
            {
                search.AssignGame(GameId.New()).IsSuccess.ShouldBeTrue();
            }
            if (status == "Closed")
            {
                search.Close().IsSuccess.ShouldBeTrue();
            }
        }
        else if (status == "Expired")
        {
            search.Expire(MatchmakingTimestamp.Create(now)).IsSuccess.ShouldBeTrue();
        }

        await SeedAsync(fixture.Services, [search], ct);

        return search;
    }

    private async Task<Guid> SeedBuildAsync(Guid playerId, CancellationToken ct)
    {
        var build = Build.Create(BuildPlayerId.Create(playerId).Value, BuildName.Create("Restart build").Value);
        await using var scope = fixture.Services.CreateAsyncScope();
        scope.ServiceProvider.GetRequiredService<IBuildsRepository>().Add(build);
        await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(BuildsWriteDbContext))
            .SaveChangesAsync(ct);

        return build.Id.Value;
    }

    private async Task<MatchSearchReadDbModel> ReadSearchAsync(Guid id, CancellationToken ct)
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<MatchmakingReadDbContext>();
        return await context.Set<MatchSearchReadDbModel>()
            .IgnoreQueryFilters()
            .SingleAsync(search => search.Id == id, ct);
    }

    private async Task<HttpResponseMessage> DeleteAsync(Guid? userId, Guid id, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, $"{Path}/{id}");
        if (userId is { } value)
        {
            request.Headers.Add("X-Test-UserId", value.ToString());
        }

        return await fixture.Client.SendAsync(request, ct);
    }

    private async Task<Result> ConfirmAsync(ConfirmMatchCommand command, CancellationToken ct)
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<IMediator>().SendAsync(command, ct);
    }
}
