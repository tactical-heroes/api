namespace PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Units.ValueObjects;

public sealed partial class UnitStackState : ValueObject
{
    private UnitStackState(int count, int currentUnitHealth)
    {
        Count = count;
        CurrentUnitHealth = currentUnitHealth;
    }

    public int Count { get; }
    public int CurrentUnitHealth { get; }

    public static Result<UnitStackState> Create(int count, int currentUnitHealth)
    {
        return count < 0 || currentUnitHealth < 0 || (count == 0) != (currentUnitHealth == 0)
            ? Result.Failure<UnitStackState>(Error.Validation(
                "A living stack must have positive count and health; a destroyed stack must have zero count and health."))
            : Result.Success(new UnitStackState(count, currentUnitHealth));
    }
}
