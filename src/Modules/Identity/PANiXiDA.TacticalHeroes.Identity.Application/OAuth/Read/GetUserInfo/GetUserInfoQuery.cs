namespace PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Read.GetUserInfo;

public sealed record GetUserInfoQuery(Guid UserId)
    : IQuery<Result<UserInfoReadModel>>;
