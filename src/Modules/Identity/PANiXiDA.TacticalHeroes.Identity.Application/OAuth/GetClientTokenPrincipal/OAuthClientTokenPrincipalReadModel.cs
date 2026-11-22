using System.Security.Claims;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Application.OAuth.GetClientTokenPrincipal;

public sealed record OAuthClientTokenPrincipalReadModel(
    IReadOnlyCollection<Claim> Claims) : IReadModel;
