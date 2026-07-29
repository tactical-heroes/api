namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.GetUserInfo;

public sealed record UserInfoReadModel(
    Guid UserId,
    string? Name,
    string? Email,
    bool? EmailVerified,
    IReadOnlyCollection<string> Roles);
