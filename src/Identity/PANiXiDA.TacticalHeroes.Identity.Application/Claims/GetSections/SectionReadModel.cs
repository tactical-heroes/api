namespace PANiXiDA.TacticalHeroes.Identity.Application.Claims.GetSections;

public sealed record SectionReadModel(string Code, string DisplayName) : IReadModel;
