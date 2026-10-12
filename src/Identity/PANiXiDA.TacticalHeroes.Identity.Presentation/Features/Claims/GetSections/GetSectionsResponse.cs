namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Claims.GetSections;

public sealed record GetSectionsResponse(string Type, IReadOnlyCollection<SectionResponse> Items);

public sealed record SectionResponse(string Code, string DisplayName);
