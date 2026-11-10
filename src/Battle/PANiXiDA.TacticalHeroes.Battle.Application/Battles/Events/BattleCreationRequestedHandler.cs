using FluentValidation.Results;

using PANiXiDA.TacticalHeroes.Battle.Application.Abstractions;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Abstractions;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Heroes;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Heroes.ValueObjects;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Units;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Units.ValueObjects;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Events;
using PANiXiDA.TacticalHeroes.Battle.Domain.Factions;
using PANiXiDA.TacticalHeroes.Battle.Domain.Games;
using PANiXiDA.TacticalHeroes.Battle.Domain.Players;

using BattleAggregate = PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Battle;
using HeroDefinitionId = PANiXiDA.TacticalHeroes.Battle.Domain.Heroes.HeroId;
using UnitDefinitionId = PANiXiDA.TacticalHeroes.Battle.Domain.Units.UnitId;

namespace PANiXiDA.TacticalHeroes.Battle.Application.Battles.Events;

public sealed class BattleCreationRequestedHandler(
    IBattlesRepository battlesRepository,
    IBuildsClient buildsClient,
    ICompendiumClient compendiumClient) : IEventHandler<BattleCreationRequested>
{
    public async Task HandleAsync(BattleCreationRequested domainEvent, CancellationToken cancellationToken)
    {
        var gameId = GameId.Create(domainEvent.GameId);
        var firstPlayerId = PlayerId.Create(domainEvent.FirstPlayerId);
        var secondPlayerId = PlayerId.Create(domainEvent.SecondPlayerId);
        var validation = Result.Combine(gameId, firstPlayerId, secondPlayerId);
        if (validation.IsFailure)
        {
            throw new ValidationException(validation.Errors.Select(error => new ValidationFailure(
                propertyName: string.Empty,
                errorMessage: error.Message)));
        }

        var existing = await battlesRepository.GetByGameIdAsync(gameId.Value, cancellationToken);
        if (existing is not null)
        {
            return;
        }

        var created = BattleAggregate.Create(
            gameId: gameId.Value,
            firstPlayerId: firstPlayerId.Value,
            secondPlayerId: secondPlayerId.Value);
        if (created.IsFailure)
        {
            throw new ValidationException(created.Errors.Select(error => new ValidationFailure(
                propertyName: string.Empty,
                errorMessage: error.Message)));
        }

        var battle = created.Value;
        var selections = new[]
        {
            (PlayerId: firstPlayerId.Value, BuildId: domainEvent.FirstBuildId),
            (PlayerId: secondPlayerId.Value, BuildId: domainEvent.SecondBuildId)
        };
        foreach (var selection in selections)
        {
            var build = await buildsClient.GetByIdAsync(selection.BuildId, cancellationToken);
            if (build is null || build.Id != selection.BuildId || build.PlayerId != selection.PlayerId.Value)
            {
                throw new InvalidOperationException("Both selected builds must exist and belong to their players.");
            }

            foreach (var id in build.UnitIds)
            {
                var unit = await compendiumClient.GetUnitByIdAsync(id, cancellationToken)
                    ?? throw new InvalidOperationException("A selected unit no longer exists in Compendium.");
                var definitionId = UnitDefinitionId.Create(unit.Id);
                var factionId = FactionId.Create(unit.FactionId);
                var stats = UnitCombatStats.Create(
                    attack: unit.Attack,
                    defense: unit.Defense,
                    maxHealth: unit.Health);
                var damage = DamageRange.Create(minimum: unit.MinimumDamage, maximum: unit.MaximumDamage);
                var stack = UnitStackState.Create(count: 1, currentUnitHealth: unit.Health);
                var traits = UnitTraits.Create(
                    initiative: unit.Initiative,
                    speed: unit.Speed,
                    morale: unit.Morale,
                    luck: unit.Luck,
                    shots: unit.Shots,
                    rangedAttackRange: unit.RangedAttackRange);
                var values = Result.Combine(definitionId, factionId, stats, damage, stack, traits);
                if (values.IsFailure)
                {
                    throw new ValidationException(values.Errors.Select(error => new ValidationFailure(
                        propertyName: string.Empty,
                        errorMessage: error.Message)));
                }

                var selected = Unit.Create(
                    battleId: battle.Id,
                    ownerPlayerId: selection.PlayerId,
                    definitionId: definitionId.Value,
                    factionId: factionId.Value,
                    stats: stats.Value,
                    damageRange: damage.Value,
                    stackState: stack.Value,
                    traits: traits.Value);
                if (selected.IsFailure)
                {
                    throw new InvalidOperationException("The unit snapshot could not be created.");
                }

                var added = battle.AddUnits([selected.Value]);
                if (added.IsFailure)
                {
                    throw new InvalidOperationException("The unit snapshot could not be added to the battle.");
                }
            }

            foreach (var id in build.HeroIds)
            {
                var hero = await compendiumClient.GetHeroByIdAsync(id, cancellationToken)
                    ?? throw new InvalidOperationException("A selected hero no longer exists in Compendium.");
                var definitionId = HeroDefinitionId.Create(hero.Id);
                var factionId = FactionId.Create(hero.FactionId);
                var stats = HeroCombatStats.Create(
                    attack: hero.Attack,
                    defense: hero.Defense,
                    minimumDamage: hero.MinimumDamage,
                    maximumDamage: hero.MaximumDamage,
                    initiative: hero.Initiative,
                    morale: hero.Morale,
                    luck: hero.Luck);
                var values = Result.Combine(definitionId, factionId, stats);
                if (values.IsFailure)
                {
                    throw new ValidationException(values.Errors.Select(error => new ValidationFailure(
                        propertyName: string.Empty,
                        errorMessage: error.Message)));
                }

                var selected = Hero.Create(
                    battleId: battle.Id,
                    ownerPlayerId: selection.PlayerId,
                    definitionId: definitionId.Value,
                    factionId: factionId.Value,
                    stats: stats.Value);
                var added = battle.AddHeroes([selected]);
                if (added.IsFailure)
                {
                    throw new InvalidOperationException("The hero snapshot could not be added to the battle.");
                }
            }
        }

        battlesRepository.Add(battle);
    }
}
