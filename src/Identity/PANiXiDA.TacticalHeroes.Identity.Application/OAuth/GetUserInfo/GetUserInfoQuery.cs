namespace PANiXiDA.TacticalHeroes.Identity.Application.OAuth.GetUserInfo;

public sealed record GetUserInfoQuery(Guid AccountId)
    : IQuery<Result<UserInfoReadModel>>;
