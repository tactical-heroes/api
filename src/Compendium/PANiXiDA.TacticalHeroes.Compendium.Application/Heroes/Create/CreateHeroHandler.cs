using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.Create;

public sealed class CreateHeroHandler(
    IHeroesRepository heroesRepository,
    IFactionsRepository factionsRepository)
    : ICommandHandler<CreateHeroCommand, Result<Guid>>
{
    public async Task<Result<Guid>> HandleAsync(
        CreateHeroCommand command,
        CancellationToken cancellationToken)
    {
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
        var factionIdResult = FactionId.Create(value: command.FactionId);
        var validationResult = Result.Combine(
            nameResult,
            descriptionResult,
            statsResult,
            moraleResult,
            luckResult,
            factionIdResult);

        if (validationResult.IsFailure)
        {
            return Result.Failure<Guid>(errors: validationResult.Errors);
        }

        var faction = await factionsRepository.GetByIdAsync(
            id: factionIdResult.Value,
            cancellationToken: cancellationToken);

        if (faction is null)
        {
            return Result.Failure<Guid>(
                error: Error.NotFound(message: "Faction was not found."));
        }

        var hero = Hero.Create(
            name: nameResult.Value,
            description: descriptionResult.Value,
            stats: statsResult.Value,
            morale: moraleResult.Value,
            luck: luckResult.Value,
            factionId: factionIdResult.Value);

        await heroesRepository.AddAsync(
            aggregateRoot: hero,
            cancellationToken: cancellationToken);

        return Result.Success(value: hero.Id.Value);
    }
}
