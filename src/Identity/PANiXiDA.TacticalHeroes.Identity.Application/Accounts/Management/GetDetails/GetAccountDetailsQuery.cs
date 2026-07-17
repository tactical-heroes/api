namespace PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Management.GetDetails;

public sealed record GetAccountDetailsQuery(Guid Id)
    : IQuery<Result<AccountDetailsReadModel>>;
