using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes.Abstractions;

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
        var heroResult = Hero.Create(
            name: command.Name,
            description: command.Description,
            attack: command.Attack,
            defense: command.Defense,
            minimumDamage: command.MinimumDamage,
            maximumDamage: command.MaximumDamage,
            initiative: command.Initiative,
            morale: command.Morale,
            luck: command.Luck,
            factionId: command.FactionId);

        if (heroResult.IsFailure)
        {
            return Result.Failure<Guid>(errors: heroResult.Errors);
        }

        var faction = await factionsRepository.GetByIdAsync(
            id: heroResult.Value.FactionId,
            cancellationToken: cancellationToken);

        if (faction is null)
        {
            return Result.Failure<Guid>(
                error: Error.NotFound(message: "Faction was not found."));
        }

        await heroesRepository.AddAsync(
            aggregateRoot: heroResult.Value,
            cancellationToken: cancellationToken);

        return Result.Success(value: heroResult.Value.Id.Value);
    }
}
