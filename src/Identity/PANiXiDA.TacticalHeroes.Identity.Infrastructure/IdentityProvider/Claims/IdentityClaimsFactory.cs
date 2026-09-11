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
        var additionalClaims = user.Claims
            .Select(claim => (claim.ClaimType, claim.ClaimValue))
            .Concat(user.Roles.SelectMany(userRole =>
                userRole.Role?.Claims.Select(claim => (claim.ClaimType, claim.ClaimValue)) ?? []));

        return Create(
            id: user.Id,
            userName: user.UserName,
            email: user.Email,
            securityStamp: user.SecurityStamp,
            securityStampClaimType: identityOptions.ClaimsIdentity.SecurityStampClaimType,
            roleNames: user.Roles.Select(userRole => userRole.Role?.Name),
            additionalClaims: additionalClaims);
    }

    internal static IReadOnlyCollection<Claim> Create(
        UserReadDbModel user)
    {
        var additionalClaims = user.Claims
            .Select(claim => (claim.ClaimType, claim.ClaimValue))
            .Concat(user.Roles.SelectMany(userRole =>
                userRole.Role?.Claims.Select(claim => (claim.ClaimType, claim.ClaimValue)) ?? []));

        return Create(
            id: user.Id,
            userName: user.UserName,
            email: user.Email,
            securityStamp: user.SecurityStamp,
            securityStampClaimType: DefaultSecurityStampClaimType,
            roleNames: user.Roles.Select(userRole => userRole.Role?.Name),
            additionalClaims: additionalClaims);
    }

    private static IReadOnlyCollection<Claim> Create(
        Guid id,
        string? userName,
        string? email,
        string? securityStamp,
        string securityStampClaimType,
        IEnumerable<string?> roleNames,
        IEnumerable<(string? Type, string? Value)> additionalClaims)
    {
        var claims = new List<Claim>
        {
            new(type: OpenIddictConstants.Claims.Subject, value: id.ToString())
        };

        AddIfPresent(claims: claims, type: OpenIddictConstants.Claims.Name, value: userName);
        AddIfPresent(claims: claims, type: OpenIddictConstants.Claims.Email, value: email);
        AddIfPresent(claims: claims, type: securityStampClaimType, value: securityStamp);

        claims.AddRange(
            roleNames
                .Where(roleName => !string.IsNullOrWhiteSpace(roleName))
                .Select(roleName => new Claim(type: OpenIddictConstants.Claims.Role, value: roleName!)));
        claims.AddRange(ToClaims(claims: additionalClaims));

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
