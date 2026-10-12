namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Claims.GetSections;

public sealed record GetSectionsResponse(string Type, IReadOnlyCollection<SectionResponse> Items);
