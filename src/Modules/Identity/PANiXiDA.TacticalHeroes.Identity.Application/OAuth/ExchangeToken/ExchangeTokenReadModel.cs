using System.Security.Claims;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Application.OAuth.ExchangeToken;

public sealed record ExchangeTokenReadModel(
    IReadOnlyCollection<Claim> Claims) : IReadModel;
