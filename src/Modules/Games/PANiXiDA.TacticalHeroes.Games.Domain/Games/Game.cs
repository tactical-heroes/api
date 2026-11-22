using PANiXiDA.TacticalHeroes.Games.Domain.Battles;
using PANiXiDA.TacticalHeroes.Games.Domain.Games.Entities.GameParticipants;
using PANiXiDA.TacticalHeroes.Games.Domain.Games.Entities.GameParticipants.ValueObjects;
using PANiXiDA.TacticalHeroes.Games.Domain.Games.Enumerations;
using PANiXiDA.TacticalHeroes.Games.Domain.Games.Events;
using PANiXiDA.TacticalHeroes.Games.Domain.Games.ValueObjects;
using PANiXiDA.TacticalHeroes.Games.Domain.MatchSearches;

using ResultFactory = PANiXiDA.Core.ResultPattern.Result;

namespace PANiXiDA.TacticalHeroes.Games.Domain.Games;

public sealed class Game : AggregateRoot<GameId>
{
    private readonly List<GameParticipant> _participants = [];

    private Game(GameId id, GameMode mode) : base(id)
    {
        Mode = mode;
        Status = GameStatus.WaitingForPlayers;
    }

    public GameMode Mode { get; private set; }
    public GameStatus Status { get; private set; }
    public BattleId? BattleId { get; private set; }
    public GameTimestamp? StartedAt { get; private set; }
    public GameTimestamp? FinishedAt { get; private set; }
    public GameResult? Result { get; private set; }
    public IReadOnlyCollection<GameParticipant> Participants => _participants.AsReadOnly();

    public static Result<Game> Create(
        GameMode mode,
        GameParticipantSelection firstParticipant,
        GameParticipantSelection secondParticipant,
        MatchSearchId firstMatchSearchId,
        MatchSearchId secondMatchSearchId)
    {
        if (firstMatchSearchId == secondMatchSearchId)
        {
            return ResultFactory.Failure<Game>(Error.Validation("A game requires two different match searches."));
        }

        if (firstParticipant.PlayerId == secondParticipant.PlayerId)
        {
            return ResultFactory.Failure<Game>(Error.Validation("A game requires two different players."));
        }

        var game = new Game(GameId.New(), mode);

        game._participants.Add(GameParticipant.Create(
            gameId: game.Id,
            playerId: firstParticipant.PlayerId,
            buildId: firstParticipant.BuildId,
            rating: firstParticipant.Rating));

        game._participants.Add(GameParticipant.Create(
            gameId: game.Id,
            playerId: secondParticipant.PlayerId,
            buildId: secondParticipant.BuildId,
            rating: secondParticipant.Rating));

        game.AddDomainEvent(new GameCreated(
            GameId: game.Id.Value,
            FirstMatchSearchId: firstMatchSearchId.Value,
            SecondMatchSearchId: secondMatchSearchId.Value));

        return ResultFactory.Success(game);
    }

    public Result Start(BattleId battleId, GameTimestamp now)
    {
        if (Status == GameStatus.InProgress && BattleId == battleId)
        {
            return ResultFactory.Success();
        }

        if (Status != GameStatus.WaitingForPlayers)
        {
            return ResultFactory.Failure(Error.Conflict("Only a waiting game can be started."));
        }

        BattleId = battleId;
        StartedAt = now;
        Status = GameStatus.InProgress;

        return ResultFactory.Success();
    }

    public Result Finish(BattleId battleId, GameResult result, GameTimestamp now)
    {
        if (BattleId != battleId)
        {
            return ResultFactory.Failure(Error.Conflict("The result must belong to the game's battle."));
        }

        if (Status == GameStatus.Finished && Result == result)
        {
            return ResultFactory.Success();
        }

        if (Status != GameStatus.InProgress)
        {
            return ResultFactory.Failure(Error.Conflict("Only a game in progress can be finished."));
        }

        if (result.WinnerPlayerId is { } winner && !_participants.Any(participant => participant.PlayerId == winner))
        {
            return ResultFactory.Failure(Error.Validation("The winner must be a participant in this game."));
        }

        if (now.Value < StartedAt!.Value)
        {
            return ResultFactory.Failure(Error.Validation("A game cannot finish before it starts."));
        }

        Result = result;
        FinishedAt = now;
        Status = GameStatus.Finished;

        AddDomainEvent(new GameFinished(
            GameId: Id.Value,
            BattleId: battleId.Value,
            Mode: Mode.Name,
            WinnerPlayerId: result.WinnerPlayerId?.Value,
            FinishedAt: now.Value));

        return ResultFactory.Success();
    }

    public Result Cancel(GameTimestamp now)
    {
        if (Status == GameStatus.Cancelled)
        {
            return ResultFactory.Success();
        }

        if (Status != GameStatus.WaitingForPlayers)
        {
            return ResultFactory.Failure(Error.Conflict("Only a waiting game can be cancelled."));
        }

        FinishedAt = now;
        Status = GameStatus.Cancelled;

        return ResultFactory.Success();
    }
}
