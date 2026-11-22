namespace PANiXiDA.TacticalHeroes.Modules.Identity.Application.OAuth.ExchangeToken;

public sealed record ExchangeTokenQuery(Guid UserId)
    : IQuery<Result<ExchangeTokenReadModel>>;
