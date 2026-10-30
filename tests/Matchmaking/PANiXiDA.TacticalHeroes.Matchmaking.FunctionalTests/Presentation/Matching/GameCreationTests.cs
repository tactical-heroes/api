using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.Core.Application.Persistence;
using PANiXiDA.Core.Application.Persistence.Errors;

using PANiXiDA.TacticalHeroes.Games.Domain.Games;
using PANiXiDA.TacticalHeroes.Games.Domain.Games.Events;
using PANiXiDA.TacticalHeroes.Games.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Events;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.ValueObjects;
using PANiXiDA.TacticalHeroes.Matchmaking.FunctionalTests.Presentation.Expiration;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Core;

using Wolverine;
using Wolverine.Runtime;
using Wolverine.Tracking;

using static PANiXiDA.TacticalHeroes.Matchmaking.FunctionalTests.Presentation.Matching.MatchmakingTestSupport;

using CreatedContract = PANiXiDA.TacticalHeroes.Games.Contracts.Games.GameCreated;
using FoundContract = PANiXiDA.TacticalHeroes.Matchmaking.Contracts.MatchSearches.MatchFound;

namespace PANiXiDA.TacticalHeroes.Matchmaking.FunctionalTests.Presentation.Matching;

[Collection(FunctionalTestCollectionDefinition.Name)]
public sealed class GameCreationTests(FunctionalTestFixture fixture)
{
    private static readonly TimeSpan s_timeout = TimeSpan.FromSeconds(30);

    [Theory(DisplayName = "MatchFound should create one game and assign both searches when pair is confirmed")]
    [InlineData("RankedDuel")]
    [InlineData("CasualDuel")]
    public async Task PublishAsync_Should_CreateGameAndAssignBothSearches_When_PairIsConfirmed(string mode)
    {
        var ct = TestContext.Current.CancellationToken;
        var first = CreateSearch(mode: mode);
        var second = CreateSearch(1510, mode);
        var now = MatchmakingTimestamp.Create(DateTimeOffset.UtcNow);
        first.Matched(now);
        second.Matched(now);
        await SeedAsync(fixture.Services, [first, second], ct);
        var found = new FoundContract(
            first.Id.Value,
            first.PlayerId.Value,
            first.BuildId.Value,
            first.Rating.Value,
            second.Id.Value,
            second.PlayerId.Value,
            second.BuildId.Value,
            second.Rating.Value,
            mode,
            now.Value);

        var tracked = await fixture.Services.TrackActivity().Timeout(s_timeout).PublishMessageAndWaitAsync(found);

        var created = tracked.FindSingleTrackedMessageOfType<CreatedContract>();
        var domainCreated = tracked.FindSingleTrackedMessageOfType<GameCreated>();
        created.Id.ShouldBe(domainCreated.Id);
        created.OccurredOnUtc.ShouldBe(domainCreated.OccurredOnUtc);
        created.FirstMatchSearchId.ShouldBe(first.Id.Value);
        created.SecondMatchSearchId.ShouldBe(second.Id.Value);
        var rows = await ReadAsync(fixture.Services, [first.Id.Value, second.Id.Value], ct);
        rows.ShouldAllBe(search => search.GameId == created.GameId && search.Status == "Matched");
        await using var scope = fixture.Services.CreateAsyncScope();
        var game = await scope.ServiceProvider.GetRequiredService<GamesWriteDbContext>()
            .Set<Game>()
            .AsNoTracking()
            .SingleAsync(item => item.Id == GameId.Create(created.GameId).Value, ct);
        game.Mode.Name.ShouldBe(mode);
        game.Status.Name.ShouldBe("WaitingForPlayers");
        game.Participants.Count.ShouldBe(2);
        var firstParticipant = game.Participants.Single(item => item.PlayerId.Value == first.PlayerId.Value);
        firstParticipant.BuildId.Value.ShouldBe(first.BuildId.Value);
        firstParticipant.Rating.Value.ShouldBe(first.Rating.Value);
        var secondParticipant = game.Participants.Single(item => item.PlayerId.Value == second.PlayerId.Value);
        secondParticipant.BuildId.Value.ShouldBe(second.BuildId.Value);
        secondParticipant.Rating.Value.ShouldBe(second.Rating.Value);

        await fixture.Services.TrackActivity().Timeout(s_timeout).PublishMessageAndWaitAsync(created);

        var repeated = await ReadAsync(fixture.Services, [first.Id.Value, second.Id.Value], ct);
        repeated.ShouldAllBe(search => search.GameId == created.GameId);
        var changes = fixture.Services.GetRequiredService<MatchSearchStateChangedProbe>().Events;
        changes.Count(item => item.MatchSearchId == first.Id.Value && item.GameId == created.GameId).ShouldBe(1);
        changes.Count(item => item.MatchSearchId == second.Id.Value && item.GameId == created.GameId).ShouldBe(1);
    }

    [Fact(DisplayName = "Inbox should prevent a second game when same match envelope is redelivered")]
    public async Task PublishAsync_Should_PreventSecondGame_When_SameMatchEnvelopeIsRedelivered()
    {
        var ct = TestContext.Current.CancellationToken;
        var first = CreateSearch();
        var second = CreateSearch();
        var now = MatchmakingTimestamp.Create(DateTimeOffset.UtcNow);
        first.Matched(now);
        second.Matched(now);
        await SeedAsync(fixture.Services, [first, second], ct);
        var found = new FoundContract(
            first.Id.Value,
            first.PlayerId.Value,
            first.BuildId.Value,
            first.Rating.Value,
            second.Id.Value,
            second.PlayerId.Value,
            second.BuildId.Value,
            second.Rating.Value,
            first.Mode.Name,
            now.Value);
        var runtime = fixture.Services.GetRequiredService<IWolverineRuntime>();
        var envelope = runtime.RoutingFor(typeof(FoundContract)).RouteForPublish(found, new DeliveryOptions())
            .Single(item => item.Destination!.Host.Contains("games.infrastructure", StringComparison.Ordinal));
        var sender = runtime.Endpoints.GetOrBuildSendingAgent(envelope.Destination.ShouldNotBeNull());
        sender.IsDurable.ShouldBeTrue();
        var before = await GameCountAsync(ct);

        await fixture.Services.TrackActivity().Timeout(s_timeout)
            .ExecuteAndWaitAsync(_ => sender.StoreAndForwardAsync(envelope));

        var rows = await ReadAsync(fixture.Services, [first.Id.Value, second.Id.Value], ct);
        var gameId = rows.First().GameId.ShouldNotBeNull();
        rows.ShouldAllBe(search => search.GameId == gameId);
        (await GameCountAsync(ct)).ShouldBe(before + 1);
        (await runtime.Storage.Inbox.IsHandledAsync(envelope, ct)).ShouldBeTrue();
        var repeated = runtime.RoutingFor(typeof(FoundContract)).RouteForPublish(found, new DeliveryOptions())
            .Single(item => item.Destination!.Host.Contains("games.infrastructure", StringComparison.Ordinal));
        repeated.Id = envelope.Id;

        await sender.StoreAndForwardAsync(repeated);

        (await runtime.Storage.Inbox.IsHandledAsync(repeated, ct)).ShouldBeTrue();
        (await GameCountAsync(ct)).ShouldBe(before + 1);
        var after = await ReadAsync(fixture.Services, [first.Id.Value, second.Id.Value], ct);
        after.ShouldAllBe(search => search.GameId == gameId);
    }

    [Theory(DisplayName = "Game creation should preserve atomicity when game saving completes")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task InvokeAsync_Should_PreserveAtomicity_When_GameSavingCompletes(bool fail)
    {
        var ct = TestContext.Current.CancellationToken;
        var first = CreateSearch();
        var second = CreateSearch();
        var now = MatchmakingTimestamp.Create(DateTimeOffset.UtcNow);
        first.Matched(now);
        second.Matched(now);
        await SeedAsync(fixture.Services, [first, second], ct);
        var request = new GameCreationRequested(
            first.Id.Value,
            first.PlayerId.Value,
            first.BuildId.Value,
            first.Rating.Value,
            second.Id.Value,
            second.PlayerId.Value,
            second.BuildId.Value,
            second.Rating.Value,
            first.Mode.Name,
            now.Value);
        var barrier = fixture.Services.GetRequiredService<GameCreationSaveBarrier>();
        var reached = barrier.PauseNextSave(fail);
        var before = await GameCountAsync(ct);

        var creation = fixture.Services.GetRequiredService<IMessageBus>().InvokeAsync(request, ct);
        try
        {
            await reached.WaitAsync(s_timeout, ct);
            (await GameCountAsync(ct)).ShouldBe(before);
            var rows = await ReadAsync(fixture.Services, [first.Id.Value, second.Id.Value], ct);
            rows.ShouldAllBe(search => search.GameId == null);
        }
        finally
        {
            barrier.Resume();
        }

        if (fail)
        {
            await Should.ThrowAsync<InvalidOperationException>(() => creation);
            (await GameCountAsync(ct)).ShouldBe(before);
            var rows = await ReadAsync(fixture.Services, [first.Id.Value, second.Id.Value], ct);
            rows.ShouldAllBe(search => search.GameId == null);
            return;
        }

        await creation;
        await WaitForAssignmentAsync(first.Id.Value, second.Id.Value, ct);
        (await GameCountAsync(ct)).ShouldBe(before + 1);
    }

    [Fact(DisplayName = "Assignment should roll back both searches and events when concurrent close wins")]
    public async Task InvokeAsync_Should_RollBackBothSearchesAndEvents_When_ConcurrentCloseWins()
    {
        var ct = TestContext.Current.CancellationToken;
        var first = CreateSearch();
        var second = CreateSearch();
        var now = MatchmakingTimestamp.Create(DateTimeOffset.UtcNow);
        first.Matched(now);
        second.Matched(now);
        await SeedAsync(fixture.Services, [first, second], ct);
        var barrier = fixture.Services.GetRequiredService<ConcurrentMatchSaveBarrier>();
        var reached = barrier.PauseNextMatchSave();
        var gameId = Guid.CreateVersion7();
        var request = new GameAssignmentRequested(gameId, first.Id.Value, second.Id.Value);

        var assignment = fixture.Services.GetRequiredService<IMessageBus>().InvokeAsync(request, ct);
        try
        {
            await reached.WaitAsync(s_timeout, ct);
            await using var scope = fixture.Services.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<IMatchSearchesRepository>();
            var stored = (await repository.GetByIdAsync(second.Id, ct)).ShouldNotBeNull();
            stored.Close().IsSuccess.ShouldBeTrue();
            repository.Update(stored);
            await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(MatchmakingWriteDbContext))
                .SaveChangesAsync(ct);
        }
        finally
        {
            barrier.ResumeMatchSave();
        }

        await Should.ThrowAsync<ConcurrencyException>(() => assignment);

        var rows = await ReadAsync(fixture.Services, [first.Id.Value, second.Id.Value], ct);
        rows.ShouldAllBe(search => search.GameId == null);
        rows.Single(search => search.Id == first.Id.Value).Status.ShouldBe("Matched");
        rows.Single(search => search.Id == second.Id.Value).Status.ShouldBe("Closed");
        fixture.Services.GetRequiredService<MatchSearchStateChangedProbe>().Events
            .ShouldNotContain(item => item.GameId == gameId);
    }

    private async Task<int> GameCountAsync(CancellationToken ct)
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<GamesWriteDbContext>().Set<Game>().CountAsync(ct);
    }

    private async Task WaitForAssignmentAsync(Guid firstId, Guid secondId, CancellationToken ct)
    {
        var deadline = DateTimeOffset.UtcNow.Add(s_timeout);
        while (DateTimeOffset.UtcNow < deadline)
        {
            var rows = await ReadAsync(fixture.Services, [firstId, secondId], ct);
            if (rows.Count == 2 && rows.All(search => search.GameId is not null))
            {
                return;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(50), ct);
        }

        throw new TimeoutException("Game assignment did not complete.");
    }
}
