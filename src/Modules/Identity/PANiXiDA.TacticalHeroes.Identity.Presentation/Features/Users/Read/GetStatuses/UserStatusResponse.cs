namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.Read.GetStatuses;

public sealed record UserStatusResponse(
    int Id,
    string Name,
    string DisplayName);
