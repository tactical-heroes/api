namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.GetDetails;

public sealed record GetUserDetailsQuery(Guid Id)
    : IQuery<Result<UserDetailsReadModel>>;
