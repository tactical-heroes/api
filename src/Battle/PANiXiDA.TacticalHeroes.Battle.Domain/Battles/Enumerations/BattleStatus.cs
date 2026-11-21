using PANiXiDA.Core.Domain.Enumerations;

namespace PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Enumerations;

public sealed partial class BattleStatus : Enumeration<BattleStatus>
{
    public const int MaxLength = 32;

    public static readonly BattleStatus Preparing = new(id: 1, name: nameof(Preparing));
    public static readonly BattleStatus InProgress = new(id: 2, name: nameof(InProgress));

    private BattleStatus(int id, string name) : base(id, name)
    {
    }

    public static Result<BattleStatus> Create(string value)
    {
        return !string.IsNullOrWhiteSpace(value) && TryFromName(value, out var status) && status is not null
            ? Result.Success(status)
            : Result.Failure<BattleStatus>(Error.Validation("Battle status is invalid."));
    }
}
