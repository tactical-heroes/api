using System.Security.Claims;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.Common.ReadModels;

public sealed record AuthenticatedUserReadModel(
    Guid Id,
    string Email,
    string UserName,
    string? SecurityStamp,
    IReadOnlyCollection<Claim> Claims) : IReadModel;
