namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.GetUserInfo;

public sealed record GetUserInfoQuery(Guid UserId)
    : IQuery<Result<UserInfoReadModel>>;
