namespace PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Abstractions;

public interface IRandomSource
{
    long NextInt64(long minimumInclusive, long maximumExclusive);
}
