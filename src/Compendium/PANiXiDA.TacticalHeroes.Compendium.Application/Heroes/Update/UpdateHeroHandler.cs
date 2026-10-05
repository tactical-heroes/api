using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.Update;

public sealed class UpdateHeroHandler(
    IHeroesRepository heroesRepository,
    IFactionsRepository factionsRepository)
    : ICommandHandler<UpdateHeroCommand, Result>
{
    public async Task<Result> HandleAsync(
        UpdateHeroCommand command,
        CancellationToken cancellationToken)
    {
        var idResult = HeroId.Create(value: command.Id);

        if (idResult.IsFailure)
        {
            return Result.Failure(errors: idResult.Errors);
        }

        var hero = await heroesRepository.GetByIdAsync(
            id: idResult.Value,
            cancellationToken: cancellationToken);

        if (hero is null)
        {
            return Result.Failure(
                error: Error.NotFound(message: "Hero was not found."));
        }

        var factionIdResult = FactionId.Create(value: command.FactionId);

        if (factionIdResult.IsFailure)
        {
            return Result.Failure(errors: factionIdResult.Errors);
        }

        var faction = await factionsRepository.GetByIdAsync(
            id: factionIdResult.Value,
            cancellationToken: cancellationToken);

        if (faction is null)
        {
            return Result.Failure(
                error: Error.NotFound(message: "Faction was not found."));
        }

        var nameResult = HeroName.Create(value: command.Name);
        var descriptionResult = HeroDescription.Create(value: command.Description);
        var statsResult = HeroCombatStats.Create(
            attack: command.Attack,
            defense: command.Defense,
            minimumDamage: command.MinimumDamage,
            maximumDamage: command.MaximumDamage,
            initiative: command.Initiative);
        var moraleResult = HeroMorale.Create(value: command.Morale);
        var luckResult = HeroLuck.Create(value: command.Luck);
        var validationResult = Result.Combine(
            nameResult,
            descriptionResult,
            statsResult,
            moraleResult,
            luckResult,
            factionIdResult);

        if (validationResult.IsFailure)
        {
            return Result.Failure(errors: validationResult.Errors);
        }

        hero.Update(
            name: nameResult.Value,
            description: descriptionResult.Value,
            stats: statsResult.Value,
            morale: moraleResult.Value,
            luck: luckResult.Value,
            factionId: factionIdResult.Value);

        await heroesRepository.UpdateAsync(
            aggregateRoot: hero,
            cancellationToken: cancellationToken);

        return Result.Success();
    }
}
