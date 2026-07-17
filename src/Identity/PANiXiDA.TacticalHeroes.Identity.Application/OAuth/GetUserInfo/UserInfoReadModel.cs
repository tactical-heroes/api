namespace PANiXiDA.TacticalHeroes.Identity.Application.OAuth.GetUserInfo;

public sealed record UserInfoReadModel(
    Guid AccountId,
    string? Name,
    string? Email,
    bool? EmailVerified,
    IReadOnlyCollection<string> Roles);
