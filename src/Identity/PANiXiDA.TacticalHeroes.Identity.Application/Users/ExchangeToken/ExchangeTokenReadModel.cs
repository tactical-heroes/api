using System.Security.Claims;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.ExchangeToken;

public sealed record ExchangeTokenReadModel(
    IReadOnlyCollection<Claim> Claims);
