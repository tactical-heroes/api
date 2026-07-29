using System.Security.Claims;

namespace PANiXiDA.TacticalHeroes.Identity.Application.OAuth.ExchangeToken;

public sealed record ExchangeTokenReadModel(
    IReadOnlyCollection<Claim> Claims);
