using System.Security.Claims;

using Microsoft.AspNetCore.Identity;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Claims;

internal static class IdentityClaimsFactory
{
    private static readonly string SecurityStampClaimType =
        new IdentityOptions().ClaimsIdentity.SecurityStampClaimType;

    internal static IReadOnlyCollection<Claim> Create(
        ApplicationUser user,
        IdentityOptions identityOptions)
    {
        return Create(
            id: user.Id,
            userName: user.UserName,
            email: user.Email,
            securityStamp: user.SecurityStamp,
            securityStampClaimType: identityOptions.ClaimsIdentity.SecurityStampClaimType,
            userClaims: user.Claims.Select(claim => (claim.ClaimType, claim.ClaimValue)),
            roleNames: user.Roles.Select(userRole => userRole.Role?.Name),
            roleClaims: user.Roles.SelectMany(userRole =>
                userRole.Role?.Claims.Select(claim => (claim.ClaimType, claim.ClaimValue)) ?? []));
    }

    internal static IReadOnlyCollection<Claim> Create(UserReadDbModel user)
    {
        return Create(
            id: user.Id,
            userName: user.UserName,
            email: user.Email,
            securityStamp: user.SecurityStamp,
            securityStampClaimType: SecurityStampClaimType,
            userClaims: user.Claims.Select(claim => (claim.ClaimType, claim.ClaimValue)),
            roleNames: user.Roles.Select(userRole => userRole.Role?.Name),
            roleClaims: user.Roles.SelectMany(userRole =>
                userRole.Role?.Claims.Select(claim => (claim.ClaimType, claim.ClaimValue)) ?? []));
    }

    private static IReadOnlyCollection<Claim> Create(
        Guid id,
        string? userName,
        string? email,
        string? securityStamp,
        string securityStampClaimType,
        IEnumerable<(string? Type, string? Value)> userClaims,
        IEnumerable<string?> roleNames,
        IEnumerable<(string? Type, string? Value)> roleClaims)
    {
        var claims = new List<Claim>
        {
            new(type: OpenIddictConstants.Claims.Subject, value: id.ToString())
        };

        AddIfPresent(claims, OpenIddictConstants.Claims.Name, userName);
        AddIfPresent(claims, OpenIddictConstants.Claims.Email, email);
        AddIfPresent(claims, securityStampClaimType, securityStamp);

        claims.AddRange(
            roleNames
                .Where(roleName => !string.IsNullOrWhiteSpace(roleName))
                .Select(roleName => new Claim(type: OpenIddictConstants.Claims.Role, value: roleName!)));
        claims.AddRange(ToClaims(claims: userClaims));
        claims.AddRange(ToClaims(claims: roleClaims));

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
}
