namespace PANiXiDA.TacticalHeroes.Modules.Identity.Application.Users.GetDetails;

public sealed record GetUserDetailsQuery(Guid Id)
    : IQuery<Result<UserDetailsReadModel>>;
