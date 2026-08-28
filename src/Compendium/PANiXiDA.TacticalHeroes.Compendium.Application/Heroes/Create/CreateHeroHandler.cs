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
        var heroResult = Hero.Create(new HeroAttributes
        {
            Name = command.Name,
            Description = command.Description,
            Attack = command.Attack,
            Defense = command.Defense,
            MinimumDamage = command.MinimumDamage,
            MaximumDamage = command.MaximumDamage,
            Initiative = command.Initiative,
            Morale = command.Morale,
            Luck = command.Luck,
            FactionId = command.FactionId
        });

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
