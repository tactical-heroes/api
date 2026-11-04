using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.Core.Application.Persistence;
using PANiXiDA.Core.ResultPattern;

using PANiXiDA.TacticalHeroes.Builds.Domain.Builds.Abstractions;
using PANiXiDA.TacticalHeroes.Builds.Domain.Builds.ValueObjects;
using PANiXiDA.TacticalHeroes.Builds.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Watch;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Builds;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Games;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Enumerations;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Events;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.ValueObjects;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Players;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Matchmaking.Presentation.Features.MatchSearches.Watch;
using PANiXiDA.TacticalHeroes.Players.Domain.Players.Abstractions;
using PANiXiDA.TacticalHeroes.Players.Domain.Users;
using PANiXiDA.TacticalHeroes.Players.Infrastructure.Persistence.Core;

using Wolverine;
using Wolverine.Tracking;

using Build = PANiXiDA.TacticalHeroes.Builds.Domain.Builds.Build;
using BuildPlayerId = PANiXiDA.TacticalHeroes.Builds.Domain.Players.PlayerId;
using Player = PANiXiDA.TacticalHeroes.Players.Domain.Players.Player;

namespace PANiXiDA.TacticalHeroes.Matchmaking.FunctionalTests.Presentation.Features.MatchSearches.Watch;

[Collection(FunctionalTestCollectionDefinition.Name)]
public sealed class WatchMatchSearchEndpointTests(FunctionalTestFixture fixture)
{
    private const string Path = "/api/v1/match-searches/state";

    private static readonly TimeSpan s_timeout = TimeSpan.FromSeconds(30);

    [Fact(DisplayName = "Endpoint should require authorization and upgrade when request is http")]
    public async Task Get_Should_RequireAuthorizationAndUpgrade_When_RequestIsHttp()
    {
        using var anonymous = await fixture.Client.GetAsync(Path, TestContext.Current.CancellationToken);
        anonymous.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        using var negotiation = await fixture.Client.PostAsync(
            $"{Path}/negotiate?negotiateVersion=1", null, TestContext.Current.CancellationToken);
        negotiation.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        using var request = new HttpRequestMessage(HttpMethod.Get, Path);
        request.Headers.Add("X-Test-UserId", Guid.NewGuid().ToString());

        using var authenticated = await fixture.Client.SendAsync(request, TestContext.Current.CancellationToken);

        authenticated.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "Endpoint should send empty state when no search exists")]
    public async Task ConnectAsync_Should_SendEmptyState_When_NoSearchExists()
    {
        var player = await SeedPlayerAsync();
        await using var socket = await ConnectAsync(player.UserId.Value);

        var state = await ReadAsync(socket);

        state.MatchSearchId.ShouldBeNull();
        state.IsActive.ShouldBeFalse();
    }

    [Fact(DisplayName = "Endpoint should stream and recover state when search changes")]
    public async Task ConnectAsync_Should_StreamAndRecoverState_When_SearchChanges()
    {
        var player = await SeedPlayerAsync();
        var build = await SeedBuildAsync(player.Id.Value);
        await using var first = await ConnectAsync(player.UserId.Value);
        await using var second = await ConnectAsync(player.UserId.Value);
        await ReadAsync(first);
        await ReadAsync(second);

        await StartAsync(player, build);
        var searching = await ReadAsync(first);
        (await ReadAsync(second)).ShouldBe(searching);
        searching.Status.ShouldBe("Searching");
        searching.IsActive.ShouldBeTrue();
        searching.ExpiresAt.ShouldBe(searching.QueuedAt!.Value.AddMinutes(5));

        await CancelAsync(player, searching.MatchSearchId!.Value);
        var cancelled = await ReadAsync(first, searching);
        (await ReadAsync(second, searching)).ShouldBe(cancelled);
        cancelled.Status.ShouldBe("Cancelled");
        cancelled.IsActive.ShouldBeFalse();

        await using var reconnected = await ConnectAsync(player.UserId.Value);
        (await ReadAsync(reconnected)).ShouldBe(cancelled);
        await StartAsync(player, build);
        var newer = await ReadAsync(first, cancelled);
        newer.MatchSearchId.ShouldNotBe(searching.MatchSearchId);
        var delayed = new MatchSearchStateChanged(searching.MatchSearchId.Value, player.Id.Value, "Searching", null);
        await fixture.Services.TrackActivity()
            .ExecuteAndWaitAsync(context => context.PublishAsync(delayed).AsTask());
        await CancelAsync(player, newer.MatchSearchId!.Value);
        var next = await ReadAsync(first, newer);
        next.MatchSearchId.ShouldBe(newer.MatchSearchId);
        next.Status.ShouldBe("Cancelled");
    }

    [Fact(DisplayName = "Endpoint should read only own state when another player has an active search")]
    public async Task ConnectAsync_Should_ReadOnlyOwnState_When_AnotherPlayerHasAnActiveSearch()
    {
        var owner = await SeedPlayerAsync();
        var other = await SeedPlayerAsync();
        await SeedSearchAsync(other.Id.Value);
        await using var socket = await ConnectAsync(owner.UserId.Value);

        (await ReadAsync(socket)).MatchSearchId.ShouldBeNull();
        await StartAsync(owner, await SeedBuildAsync(owner.Id.Value));
        var state = await ReadAsync(socket);

        state.Status.ShouldBe("Searching");
        await using var scope = fixture.Services.CreateAsyncScope();
        var own = await scope.ServiceProvider.GetRequiredService<IMatchSearchesReadRepository>()
            .GetStateByPlayerIdAsync(owner.Id.Value, TestContext.Current.CancellationToken);
        own.ShouldNotBeNull();
        state.MatchSearchId.ShouldBe(own.MatchSearchId);
        state.Status.ShouldBe(own.Status);
        state.GameId.ShouldBe(own.GameId);
        state.QueuedAt.ShouldBe(own.QueuedAt);
        state.ExpiresAt.ShouldBe(own.ExpiresAt);
        state.IsActive.ShouldBeTrue();
    }

    [Fact(DisplayName = "Endpoint should preserve changes when commit races with snapshot")]
    public async Task ConnectAsync_Should_PreserveChanges_When_CommitRacesWithSnapshot()
    {
        var player = await SeedPlayerAsync();
        var barrier = fixture.Services.GetRequiredService<MatchSearchSnapshotBarrier>();
        barrier.Arm();
        var connecting = ConnectAsync(player.UserId.Value);
        try
        {
            await barrier.Reached.WaitAsync(s_timeout, TestContext.Current.CancellationToken);
            await StartAsync(player, await SeedBuildAsync(player.Id.Value));
        }
        finally
        {
            barrier.Release();
        }

        await using var socket = await connecting;
        var snapshot = await ReadAsync(socket);
        snapshot.IsActive.ShouldBeFalse();
        var changed = await ReadAsync(socket, snapshot);
        changed.Status.ShouldBe("Searching");
    }

    [Fact(DisplayName = "Endpoint should stream domain transitions when match and game state change")]
    public async Task ConnectAsync_Should_StreamDomainTransitions_When_MatchAndGameStateChange()
    {
        var player = await SeedPlayerAsync();
        var search = await SeedSearchAsync(player.Id.Value);
        await using var socket = await ConnectAsync(player.UserId.Value);
        var previous = await ReadAsync(socket);
        var gameId = GameId.New();
        Func<MatchSearch, Result>[] transitions =
        [
            aggregate => aggregate.Matched(MatchmakingTimestamp.Create(DateTimeOffset.UtcNow)),
            aggregate => aggregate.AssignGame(gameId),
            aggregate => aggregate.Close()
        ];
        foreach (var transition in transitions)
        {
            await TransitionAsync(search.Id, transition);
            var state = await ReadAsync(socket, previous);
            previous = state;
        }

        previous.Status.ShouldBe("Closed");
        previous.GameId.ShouldBe(gameId.Value);
        previous.IsActive.ShouldBeFalse();
    }

    [Fact(DisplayName = "Endpoint should receive expiration when search expires")]
    public async Task ConnectAsync_Should_ReceiveExpiration_When_SearchExpires()
    {
        var player = await SeedPlayerAsync();
        var search = await SeedSearchAsync(player.Id.Value, expired: true);
        await using var socket = await ConnectAsync(player.UserId.Value);
        await ReadAsync(socket);

        var now = MatchmakingTimestamp.Create(DateTimeOffset.UtcNow);
        await TransitionAsync(search.Id, aggregate => aggregate.Expire(now));

        var state = await ReadAsync(socket);
        state.Status.ShouldBe("Expired");
        await using var reconnected = await ConnectAsync(player.UserId.Value);
        (await ReadAsync(reconnected)).ShouldBe(state);
    }

    [Fact(DisplayName = "Hub should deliver state when publication originates on another replica")]
    public async Task ConnectAsync_Should_DeliverState_When_PublicationOriginatesOnAnotherReplica()
    {
        var player = await SeedPlayerAsync();
        await using var replica = new FunctionalTestWebApplicationFactory(false, false);
        using var replicaHttp = replica.CreateClient();
        await using var socket = await MatchSearchHubClient.ConnectAsync(
            replica, player.UserId.Value, TestContext.Current.CancellationToken);
        await ReadAsync(socket);
        await using var publisherScope = fixture.Services.CreateAsyncScope();
        var publisher = publisherScope.ServiceProvider.GetRequiredService<
            PANiXiDA.TacticalHeroes.Matchmaking.Application.Abstractions.IMatchSearchStatePublisher>();
        var expected = new MatchSearchStateReadModel(Guid.NewGuid(), "Searching", null, null, null);

        await publisher.PublishAsync(player.Id.Value, expected, TestContext.Current.CancellationToken);

        (await ReadAsync(socket)).ShouldBe(
            new MatchSearchStateResponse(expected.MatchSearchId, "Searching", null, null, null, true));
    }

    private static async Task<MatchSearchStateResponse> ReadAsync(
        MatchSearchHubClient socket, MatchSearchStateResponse? previous = null)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        timeout.CancelAfter(s_timeout);
        MatchSearchStateResponse state;
        do
        {
            state = await socket.ReadAsync(timeout.Token);
        }
        while (state == previous);

        return state;
    }

    private Task<MatchSearchHubClient> ConnectAsync(Guid userId)
    {
        return MatchSearchHubClient.ConnectAsync(fixture.Factory, userId, TestContext.Current.CancellationToken);
    }

    private async Task<Player> SeedPlayerAsync()
    {
        var player = Player.Create(UserId.New());
        await using var scope = fixture.Services.CreateAsyncScope();
        scope.ServiceProvider.GetRequiredService<IPlayersRepository>().Add(player);
        await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(PlayersWriteDbContext))
            .SaveChangesAsync(TestContext.Current.CancellationToken);
        return player;
    }

    private async Task<Build> SeedBuildAsync(Guid playerId)
    {
        var build = Build.Create(BuildPlayerId.Create(playerId).Value, BuildName.Create("WebSocket build").Value);
        await using var scope = fixture.Services.CreateAsyncScope();
        scope.ServiceProvider.GetRequiredService<IBuildsRepository>().Add(build);
        await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(BuildsWriteDbContext))
            .SaveChangesAsync(TestContext.Current.CancellationToken);
        return build;
    }

    private async Task<MatchSearch> SeedSearchAsync(Guid playerId, bool expired = false)
    {
        var search = MatchSearch.Create(
            PlayerId.Create(playerId).Value,
            BuildId.New(),
            Rating.Create(1500),
            MatchmakingMode.RankedDuel,
            MatchmakingTimestamp.Create(DateTimeOffset.UtcNow.AddMinutes(expired ? -6 : -1))).Value;
        await using var scope = fixture.Services.CreateAsyncScope();
        scope.ServiceProvider.GetRequiredService<IMatchSearchesRepository>().Add(search);
        await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(MatchmakingWriteDbContext))
            .SaveChangesAsync(TestContext.Current.CancellationToken);
        return search;
    }

    private async Task StartAsync(Player player, Build build)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/match-searches");
        request.Headers.Add("X-Test-UserId", player.UserId.Value.ToString());
        request.Content = JsonContent.Create(new { buildId = build.Id.Value, mode = "RankedDuel" });
        using var response = await fixture.Client.SendAsync(request, TestContext.Current.CancellationToken);
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    private async Task CancelAsync(Player player, Guid searchId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/match-searches/{searchId}/cancel");
        request.Headers.Add("X-Test-UserId", player.UserId.Value.ToString());
        using var response = await fixture.Client.SendAsync(request, TestContext.Current.CancellationToken);
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    private async Task TransitionAsync(MatchSearchId searchId, Func<MatchSearch, Result> transition)
    {
        var ct = TestContext.Current.CancellationToken;
        await using var scope = fixture.Services.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IMatchSearchesRepository>();
        var search = (await repository.GetByIdAsync(searchId, ct))!;
        transition(search).IsSuccess.ShouldBeTrue();
        repository.Update(search);
        await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(MatchmakingWriteDbContext))
            .SaveChangesAsync(ct);
        var bus = fixture.Services.GetRequiredService<IMessageBus>();
        foreach (var changed in search.GetDomainEvents().OfType<MatchSearchStateChanged>())
        {
            await bus.PublishAsync(changed);
        }
    }
}
