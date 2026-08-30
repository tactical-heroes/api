using System.Security.Claims;

using Microsoft.AspNetCore.Identity;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Claims;

internal static class IdentityClaimsFactory
{
    private static readonly string DefaultSecurityStampClaimType =
        new IdentityOptions().ClaimsIdentity.SecurityStampClaimType;

    internal static IReadOnlyCollection<Claim> Create(
        ApplicationUser user,
        IdentityOptions identityOptions)
    {
        return Create(
            new ClaimsData
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                SecurityStamp = user.SecurityStamp,
                SecurityStampClaimType = identityOptions.ClaimsIdentity.SecurityStampClaimType,
                UserClaims = user.Claims.Select(claim => (claim.ClaimType, claim.ClaimValue)),
                RoleNames = user.Roles.Select(userRole => userRole.Role?.Name),
                RoleClaims = user.Roles.SelectMany(userRole =>
                    userRole.Role?.Claims.Select(claim => (claim.ClaimType, claim.ClaimValue)) ?? [])
            });
    }

    internal static IReadOnlyCollection<Claim> Create(UserReadDbModel user)
    {
        return Create(
            new ClaimsData
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                SecurityStamp = user.SecurityStamp,
                SecurityStampClaimType = DefaultSecurityStampClaimType,
                UserClaims = user.Claims.Select(claim => (claim.ClaimType, claim.ClaimValue)),
                RoleNames = user.Roles.Select(userRole => userRole.Role?.Name),
                RoleClaims = user.Roles.SelectMany(userRole =>
                    userRole.Role?.Claims.Select(claim => (claim.ClaimType, claim.ClaimValue)) ?? [])
            });
    }

    private static IReadOnlyCollection<Claim> Create(ClaimsData data)
    {
        var claims = new List<Claim>
        {
            new(type: OpenIddictConstants.Claims.Subject, value: data.Id.ToString())
        };

        AddIfPresent(claims: claims, type: OpenIddictConstants.Claims.Name, value: data.UserName);
        AddIfPresent(claims: claims, type: OpenIddictConstants.Claims.Email, value: data.Email);
        AddIfPresent(claims: claims, type: data.SecurityStampClaimType, value: data.SecurityStamp);

        claims.AddRange(
            data.RoleNames
                .Where(roleName => !string.IsNullOrWhiteSpace(roleName))
                .Select(roleName => new Claim(type: OpenIddictConstants.Claims.Role, value: roleName!)));
        claims.AddRange(ToClaims(claims: data.UserClaims));
        claims.AddRange(ToClaims(claims: data.RoleClaims));

        return [.. claims.Distinct(IdentityClaimComparer.Instance)];
    }

    private static IEnumerable<Claim> ToClaims(
        IEnumerable<(string? Type, string? Value)> claims)
    {
        return claims
            .Where(claim =>
                !string.IsNullOrWhiteSpace(claim.Type) &&
                !string.IsNullOrWhiteSpace(claim.Value))
            .Select(claim => new Claim(type: claim.Type!, value: claim.Value!));
    }

    private static void AddIfPresent(
        List<Claim> claims,
        string type,
        string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            claims.Add(new Claim(type: type, value: value));
        }
    }

    private sealed record ClaimsData
    {
        public required Guid Id { get; init; }
        public required string? UserName { get; init; }
        public required string? Email { get; init; }
        public required string? SecurityStamp { get; init; }
        public required string SecurityStampClaimType { get; init; }
        public required IEnumerable<(string? Type, string? Value)> UserClaims { get; init; }
        public required IEnumerable<string?> RoleNames { get; init; }
        public required IEnumerable<(string? Type, string? Value)> RoleClaims { get; init; }
    }
}
