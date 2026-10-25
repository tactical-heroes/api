namespace PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Services;

public static class DamageRandomizer
{
    public static int Roll(int minimum, int maximum)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(minimum, maximum);

        return minimum == maximum ? minimum : (int)Random.Shared.NextInt64(minimum, (long)maximum + 1);
    }
}
