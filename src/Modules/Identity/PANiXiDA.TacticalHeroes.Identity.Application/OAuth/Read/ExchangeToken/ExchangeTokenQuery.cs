namespace PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Read.ExchangeToken;

public sealed record ExchangeTokenQuery(Guid UserId)
    : IQuery<Result<ExchangeTokenReadModel>>;
