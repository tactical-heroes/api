using System.Security.Claims;

namespace PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Read.ExchangeToken;

public sealed record ExchangeTokenReadModel(
    IReadOnlyCollection<Claim> Claims) : IReadModel;
