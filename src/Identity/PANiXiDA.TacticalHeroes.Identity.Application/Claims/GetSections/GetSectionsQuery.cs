namespace PANiXiDA.TacticalHeroes.Identity.Application.Claims.GetSections;

public sealed record GetSectionsQuery()
    : IQuery<Result<IReadOnlyCollection<SectionReadModel>>>;
