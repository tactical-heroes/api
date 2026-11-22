namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Read.GetDetails;

public sealed record GetUserDetailsQuery(Guid Id)
    : IQuery<Result<UserDetailsReadModel>>;
