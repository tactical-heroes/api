using PANiXiDA.TacticalHeroes.Identity.Application.Claims.GetSections;

using Riok.Mapperly.Abstractions;

using ApplicationClaimTypes = PANiXiDA.Core.Application.Authentication.ClaimTypes;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Claims.GetSections;

[Mapper]
internal static partial class GetSectionsMapper
{
    internal static GetSectionsQuery ToQuery() => new();

    internal static GetSectionsResponse ToResponse(IReadOnlyCollection<SectionReadModel> sections) =>
        new(ApplicationClaimTypes.Section, ToItems(sections));

    private static partial IReadOnlyCollection<SectionResponse> ToItems(
        IReadOnlyCollection<SectionReadModel> sections);
}
