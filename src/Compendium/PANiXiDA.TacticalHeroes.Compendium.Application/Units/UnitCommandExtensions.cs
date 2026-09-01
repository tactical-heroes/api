using PANiXiDA.TacticalHeroes.Compendium.Application.Units.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Units.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Units;

internal static class UnitCommandExtensions
{
    public static UnitAttributes ToUnitAttributes(this IUnitAttributesCommand command)
    {
        return new UnitAttributes
        {
            Name = command.Name,
            Description = command.Description,
            CombatStats = new UnitCombatStatsInput
            {
                Attack = command.Attack,
                Defense = command.Defense,
                Health = command.Health,
                MinimumDamage = command.MinimumDamage,
                MaximumDamage = command.MaximumDamage,
                Initiative = command.Initiative,
                Speed = command.Speed,
                Shots = command.Shots,
                RangedAttackRange = command.RangedAttackRange
            },
            Morale = command.Morale,
            Luck = command.Luck,
            FactionId = command.FactionId
        };
    }
}
