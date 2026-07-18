namespace PANiXiDA.TacticalHeroes.Identity.Application.OAuth.GetUserInfo;

public sealed record GetUserInfoQuery(Guid UserId)
    : IQuery<Result<UserInfoReadModel>>;
