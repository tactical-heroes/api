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
            userClaims: user.Claims.Select(selector: claim => (claim.ClaimType, claim.ClaimValue)),
            roleNames: user.Roles.Select(selector: userRole => userRole.Role?.Name),
            roleClaims: user.Roles.SelectMany(selector: userRole =>
                userRole.Role?.Claims.Select(selector: claim => (claim.ClaimType, claim.ClaimValue)) ?? []));
    }

    internal static IReadOnlyCollection<Claim> Create(UserReadDbModel user)
    {
        return Create(
            id: user.Id,
            userName: user.UserName,
            email: user.Email,
            securityStamp: user.SecurityStamp,
            securityStampClaimType: SecurityStampClaimType,
            userClaims: user.Claims.Select(selector: claim => (claim.ClaimType, claim.ClaimValue)),
            roleNames: user.Roles.Select(selector: userRole => userRole.Role?.Name),
            roleClaims: user.Roles.SelectMany(selector: userRole =>
                userRole.Role?.Claims.Select(selector: claim => (claim.ClaimType, claim.ClaimValue)) ?? []));
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

        AddIfPresent(claims: claims, type: OpenIddictConstants.Claims.Name, value: userName);
        AddIfPresent(claims: claims, type: OpenIddictConstants.Claims.Email, value: email);
        AddIfPresent(claims: claims, type: securityStampClaimType, value: securityStamp);

        claims.AddRange(
            collection: roleNames
                .Where(predicate: roleName => !string.IsNullOrWhiteSpace(value: roleName))
                .Select(selector: roleName => new Claim(type: OpenIddictConstants.Claims.Role, value: roleName!)));
        claims.AddRange(collection: ToClaims(claims: userClaims));
        claims.AddRange(collection: ToClaims(claims: roleClaims));

        return [.. claims.Distinct(comparer: IdentityClaimComparer.Instance)];
    }

    private static IEnumerable<Claim> ToClaims(
        IEnumerable<(string? Type, string? Value)> claims)
    {
        return claims
            .Where(predicate: claim =>
                !string.IsNullOrWhiteSpace(value: claim.Type) &&
                !string.IsNullOrWhiteSpace(value: claim.Value))
            .Select(selector: claim => new Claim(type: claim.Type!, value: claim.Value!));
    }

    private static void AddIfPresent(
        List<Claim> claims,
        string type,
        string? value)
    {
        if (!string.IsNullOrWhiteSpace(value: value))
        {
            claims.Add(item: new Claim(type: type, value: value));
        }
    }
}
