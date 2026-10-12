using PANiXiDA.Core.Application.Authentication.Abstractions;

using SectionCatalog = PANiXiDA.TacticalHeroes.Common.Authentication.Sections.Sections;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Claims.GetSections;

public sealed class GetSectionsHandler
    : IQueryHandler<GetSectionsQuery, Result<IReadOnlyCollection<SectionReadModel>>>, IRequireAuthorization
{
    public Task<Result<IReadOnlyCollection<SectionReadModel>>> HandleAsync(
        GetSectionsQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<SectionReadModel> sections =
        [
            .. SectionCatalog.All.Select(section => new SectionReadModel(section.Code, section.DisplayName))
        ];

        return Task.FromResult(result: Result.Success(value: sections));
    }
}
