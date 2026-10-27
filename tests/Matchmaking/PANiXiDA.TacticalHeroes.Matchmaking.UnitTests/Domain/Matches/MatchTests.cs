using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Games;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Matches;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Matches.Entities.MatchParticipants;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Matches.Enumerations;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Matches.Events;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchmakingTickets.Enumerations;

using static PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Matchmaking.MatchmakingTestData;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Domain.Matches;

public sealed class MatchTests
{
    [Fact(DisplayName = "Match should capture selections and request game creation when pair is matched")]
    public void Create_Should_CaptureSelectionsAndRequestGameCreation_When_PairIsMatched()
    {
        var first = Ticket();
        var second = Ticket();

        var match = Match.Create(first, second, At(1), At(60)).Value;

        match.Participants.Count.ShouldBe(2);
        match.Participants.Select(value => value.Id).Distinct().Count().ShouldBe(2);
        match.Participants.ShouldAllBe(value => value.MatchId == match.Id);
        var participant = match.Participants.Single(value => value.TicketId == first.Id);
        participant.PlayerId.ShouldBe(first.PlayerId);
        participant.BuildId.ShouldBe(first.BuildId);
        participant.Rating.ShouldBe(first.Rating);
        first.Status.ShouldBe(MatchmakingTicketStatus.Searching);
        second.Status.ShouldBe(MatchmakingTicketStatus.Searching);
        match.MatchedAt.ShouldBe(At(1));
        match.CreationDeadline.ShouldBe(At(60));
        match.Status.ShouldBe(MatchStatus.CreatingGame);
        var state = match.GetDomainEvents().OfType<MatchStateChanged>().ShouldHaveSingleItem();
        state.Status.ShouldBe(MatchStatus.CreatingGame.Name);
        var request = match.GetDomainEvents().OfType<GameCreationRequested>().ShouldHaveSingleItem();
        request.MatchId.ShouldBe(match.Id.Value);
        Should.Throw<NotSupportedException>(() => ((ICollection<MatchParticipant>)match.Participants).Clear());
    }

    [Fact(DisplayName = "Match should reject incompatible tickets when created")]
    public void Create_Should_RejectIncompatibleTickets_When_Created()
    {
        var first = Ticket();

        Match.Create(first, first, At(1), At(60)).IsFailure.ShouldBeTrue();
        Match.Create(first, Ticket(first.PlayerId.Value), At(1), At(60)).IsFailure.ShouldBeTrue();
        Match.Create(first, Ticket(mode: "other"), At(1), At(60)).IsFailure.ShouldBeTrue();
    }

    [Theory(DisplayName = "Match should reject deadline when it does not follow match time")]
    [InlineData(1)]
    [InlineData(0)]
    public void Create_Should_RejectDeadline_When_ItDoesNotFollowMatchTime(int creation)
    {
        var result = Match.Create(Ticket(), Ticket(), At(1), At(creation));

        result.IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Match should reject unavailable tickets when created")]
    public void Create_Should_RejectUnavailableTickets_When_Created()
    {
        var first = Ticket();
        var second = Ticket();

        Match.Create(first, second, At(-1), At(60)).IsFailure.ShouldBeTrue();
        Match.Create(first, Ticket(expiresAt: 1), At(1), At(60)).IsFailure.ShouldBeTrue();
        second.Reserve(MatchId.New(), At(1)).IsSuccess.ShouldBeTrue();
        Match.Create(first, second, At(2), At(60)).IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Match should bind exactly one game when delivery is retried")]
    public void Complete_Should_BindExactlyOneGame_When_DeliveryIsRetried()
    {
        var match = CreateMatch();
        var game = GameId.Create(Guid.CreateVersion7()).Value;

        match.Complete(game, At(3)).IsSuccess.ShouldBeTrue();
        var count = match.GetDomainEvents().Count;
        match.Complete(game, At(100)).IsSuccess.ShouldBeTrue();
        match.Complete(GameId.Create(Guid.CreateVersion7()).Value, At(4)).IsFailure.ShouldBeTrue();

        match.GameId.ShouldBe(game);
        match.Status.ShouldBe(MatchStatus.Ready);
        match.GetDomainEvents().Count.ShouldBe(count);
        match.GetDomainEvents().OfType<GameCreationRequested>().ShouldHaveSingleItem();
        match.Expire(At(100)).IsFailure.ShouldBeTrue();
        match.Fail().IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Match should reject result when outside creation window")]
    public void Complete_Should_RejectResult_When_OutsideCreationWindow()
    {
        var game = GameId.Create(Guid.CreateVersion7()).Value;
        var match = CreateMatch();

        match.Complete(game, At(0)).IsFailure.ShouldBeTrue();
        match.Complete(game, At(60)).IsFailure.ShouldBeTrue();
        match.Expire(At(60)).IsSuccess.ShouldBeTrue();
        match.Complete(game, At(61)).IsFailure.ShouldBeTrue();

        match.GameId.ShouldBeNull();
    }

    [Fact(DisplayName = "Match should close game creation once when retried")]
    public void Fail_Should_CloseGameCreationOnce_When_Retried()
    {
        var match = CreateMatch();

        match.Fail().IsSuccess.ShouldBeTrue();
        var count = match.GetDomainEvents().Count;
        match.Fail().IsSuccess.ShouldBeTrue();

        match.Status.ShouldBe(MatchStatus.Failed);
        match.GetDomainEvents().Count.ShouldBe(count);
        match.Complete(GameId.Create(Guid.CreateVersion7()).Value, At(3)).IsFailure.ShouldBeTrue();
        match.Expire(At(100)).IsFailure.ShouldBeTrue();
    }

    [Fact(DisplayName = "Match should expire creation and publish once when overdue")]
    public void Expire_Should_ExpireCreationAndPublishOnce_When_Overdue()
    {
        var match = CreateMatch();
        match.Expire(At(59)).IsFailure.ShouldBeTrue();

        match.Expire(At(60)).IsSuccess.ShouldBeTrue();
        var count = match.GetDomainEvents().Count;
        match.Expire(At(61)).IsSuccess.ShouldBeTrue();

        match.Status.ShouldBe(MatchStatus.Expired);
        match.GetDomainEvents().Count.ShouldBe(count);
        match.Complete(GameId.Create(Guid.CreateVersion7()).Value, At(61)).IsFailure.ShouldBeTrue();
        match.Fail().IsFailure.ShouldBeTrue();
    }
}
