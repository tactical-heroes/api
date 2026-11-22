namespace PANiXiDA.TacticalHeroes.Modules.Identity.Application.OAuth.GetUserInfo;

public sealed record GetUserInfoQuery(Guid UserId)
    : IQuery<Result<UserInfoReadModel>>;
