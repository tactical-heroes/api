using PANiXiDA.TacticalHeroes.Games.Domain.Battles;
using PANiXiDA.TacticalHeroes.Games.Domain.Builds;
using PANiXiDA.TacticalHeroes.Games.Domain.Games;
using PANiXiDA.TacticalHeroes.Games.Domain.Games.Entities.GameParticipants;
using PANiXiDA.TacticalHeroes.Games.Domain.Games.Entities.GameParticipants.ValueObjects;
using PANiXiDA.TacticalHeroes.Games.Domain.Games.Enumerations;
using PANiXiDA.TacticalHeroes.Games.Domain.Games.Events;
using PANiXiDA.TacticalHeroes.Games.Domain.Games.ValueObjects;
using PANiXiDA.TacticalHeroes.Games.Domain.Players;
using PANiXiDA.TacticalHeroes.Games.UnitTests.Games;

namespace PANiXiDA.TacticalHeroes.Games.UnitTests.Domain.Games;

public sealed class GameTests
{
    [Theory(DisplayName = "Game should start with two distinct participants when mode is supported")]
    [InlineData("RankedDuel")]
    [InlineData("CasualDuel")]
    public void Create_Should_SetTwoParticipants_When_ModeIsSupported(string mode)
    {
        var game = GameTestData.Create(GameMode.Create(mode).Value);

        game.Id.Value.Version.ShouldBe(7);
        game.Mode.Name.ShouldBe(mode);
        game.Status.ShouldBe(GameStatus.WaitingForPlayers);
        game.BattleId.ShouldBeNull();
        game.StartedAt.ShouldBeNull();
        game.FinishedAt.ShouldBeNull();
        game.Result.ShouldBeNull();
        game.Participants.Count.ShouldBe(2);
        game.Participants.Select(participant => participant.PlayerId).Distinct().Count().ShouldBe(2);
        game.Participants.Select(participant => participant.Id).Distinct().Count().ShouldBe(2);
        game.Participants.All(participant => participant.GameId == game.Id).ShouldBeTrue();
        game.Participants.Select(participant => participant.Rating.Value).ShouldBe([1000, 1200]);
        ((ICollection<GameParticipant>)game.Participants).IsReadOnly
            .ShouldBeTrue();
    }

    [Fact(DisplayName = "Game should reject duplicate participants when player ids are equal")]
    public void Create_Should_RejectDuplicateParticipants_When_PlayerIdsAreEqual()
    {
        var playerId = PlayerId.New();

        var result = Game.Create(
            GameMode.RankedDuel,
            playerId,
            BuildId.New(),
            Rating.Create(1000),
            playerId,
            BuildId.New(),
            Rating.Create(1000));

        result.ShouldHaveSingleError(ErrorType.Validation, "A game requires two different players.");
    }

    [Fact(DisplayName = "Game should preserve its battle and start time when start is repeated")]
    public void Start_Should_PreserveBattleAndTime_When_StartIsRepeated()
    {
        var game = GameTestData.Create();
        var battleId = BattleId.New();

        game.Start(battleId, GameTestData.At(10)).IsSuccess.ShouldBeTrue();
        game.Start(battleId, GameTestData.At(20)).IsSuccess.ShouldBeTrue();

        game.Status.ShouldBe(GameStatus.InProgress);
        game.BattleId.ShouldBe(battleId);
        game.StartedAt.ShouldBe(GameTestData.At(10));
        game.FinishedAt.ShouldBeNull();
        game.Result.ShouldBeNull();
        game.Cancel(GameTestData.At(30)).IsFailure.ShouldBeTrue();
        game.Start(BattleId.New(), GameTestData.At(30)).IsFailure.ShouldBeTrue();
    }

    [Theory(DisplayName = "Game should preserve its result and time when finish is repeated")]
    [InlineData(false)]
    [InlineData(true)]
    public void Finish_Should_PreserveResultAndTime_When_FinishIsRepeated(bool draw)
    {
        var game = GameTestData.Create();
        var battleId = BattleId.New();
        var winner = game.Participants.First().PlayerId;
        var result = draw ? GameResult.Draw() : GameResult.Victory(winner);
        game.Start(battleId, GameTestData.At(10));

        game.Finish(battleId, result, GameTestData.At(20)).IsSuccess.ShouldBeTrue();
        game.Finish(battleId, result, GameTestData.At(30)).IsSuccess.ShouldBeTrue();

        game.Status.ShouldBe(GameStatus.Finished);
        game.Result.ShouldBe(result);
        game.StartedAt.ShouldBe(GameTestData.At(10));
        game.FinishedAt.ShouldBe(GameTestData.At(20));
        var domainEvent = game.GetDomainEvents().ShouldHaveSingleItem().ShouldBeOfType<GameFinished>();
        domainEvent.GameId.ShouldBe(game.Id.Value);
        domainEvent.BattleId.ShouldBe(battleId.Value);
        domainEvent.Mode.ShouldBe(game.Mode.Name);
        domainEvent.WinnerPlayerId.ShouldBe(result.WinnerPlayerId?.Value);
        domainEvent.FinishedAt.ShouldBe(GameTestData.At(20).Value);
        game.Start(battleId, GameTestData.At(30)).IsFailure.ShouldBeTrue();
        game.Cancel(GameTestData.At(30)).IsFailure.ShouldBeTrue();
        var differentResult = draw ? GameResult.Victory(winner) : GameResult.Draw();
        game.Finish(battleId, differentResult, GameTestData.At(30)).IsFailure.ShouldBeTrue();
        game.Result.ShouldBe(result);
        game.GetDomainEvents().Count.ShouldBe(1);
    }

    [Fact(DisplayName = "Game should reject a result when battle is different")]
    public void Finish_Should_RejectResult_When_BattleIsDifferent()
    {
        var game = GameTestData.Create();
        game.Start(BattleId.New(), GameTestData.At(10));

        var result = game.Finish(BattleId.New(), GameResult.Draw(), GameTestData.At(20));

        result.IsFailure.ShouldBeTrue();
        game.Status.ShouldBe(GameStatus.InProgress);
        game.Result.ShouldBeNull();
        game.GetDomainEvents().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Game should reject a winner when player does not participate")]
    public void Finish_Should_RejectWinner_When_PlayerDoesNotParticipate()
    {
        var game = GameTestData.Create();
        var battleId = BattleId.New();
        game.Start(battleId, GameTestData.At(10));

        var result = game.Finish(battleId, GameResult.Victory(PlayerId.New()), GameTestData.At(20));

        result.IsFailure.ShouldBeTrue();
        game.Status.ShouldBe(GameStatus.InProgress);
        game.Result.ShouldBeNull();
        game.GetDomainEvents().ShouldBeEmpty();
    }

    [Theory(DisplayName = "Game should validate finish time when time is compared with start")]
    [InlineData(9, false)]
    [InlineData(10, true)]
    public void Finish_Should_ValidateTime_When_TimeIsComparedWithStart(int seconds, bool expected)
    {
        var game = GameTestData.Create();
        var battleId = BattleId.New();
        game.Start(battleId, GameTestData.At(10));

        var result = game.Finish(battleId, GameResult.Draw(), GameTestData.At(seconds));

        result.IsSuccess.ShouldBe(expected);
        game.Status.ShouldBe(expected ? GameStatus.Finished : GameStatus.InProgress);
    }

    [Fact(DisplayName = "Game should preserve cancellation time when cancellation is repeated")]
    public void Cancel_Should_PreserveTime_When_CancellationIsRepeated()
    {
        var game = GameTestData.Create();

        game.Cancel(GameTestData.At(10)).IsSuccess.ShouldBeTrue();
        game.Cancel(GameTestData.At(20)).IsSuccess.ShouldBeTrue();

        game.Status.ShouldBe(GameStatus.Cancelled);
        game.FinishedAt.ShouldBe(GameTestData.At(10));
        game.StartedAt.ShouldBeNull();
        game.BattleId.ShouldBeNull();
        game.Result.ShouldBeNull();
        game.Start(BattleId.New(), GameTestData.At(30)).IsFailure.ShouldBeTrue();
        game.Finish(BattleId.New(), GameResult.Draw(), GameTestData.At(30)).IsFailure.ShouldBeTrue();
        game.GetDomainEvents().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Game should reject completion when waiting for players")]
    public void Finish_Should_RejectCompletion_When_WaitingForPlayers()
    {
        var game = GameTestData.Create();

        var result = game.Finish(BattleId.New(), GameResult.Draw(), GameTestData.At(10));

        result.IsFailure.ShouldBeTrue();
        game.Status.ShouldBe(GameStatus.WaitingForPlayers);
        game.FinishedAt.ShouldBeNull();
    }
}
