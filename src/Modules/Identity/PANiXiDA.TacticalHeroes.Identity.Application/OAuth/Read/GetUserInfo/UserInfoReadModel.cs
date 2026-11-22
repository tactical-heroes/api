namespace PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Read.GetUserInfo;

public sealed record UserInfoReadModel(
    Guid UserId,
    string? Name,
    string? Email,
    bool? EmailVerified,
    IReadOnlyCollection<string> Roles) : IReadModel;
