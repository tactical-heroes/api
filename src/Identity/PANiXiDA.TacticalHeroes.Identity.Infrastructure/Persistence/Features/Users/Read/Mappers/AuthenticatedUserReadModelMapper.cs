using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Identity.Application.Users.GetAuthenticated;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.Mappers;

internal static class AuthenticatedUserReadModelMapper
{
    public static async Task<AuthenticatedUserReadModel?> GetByIdAsync(
        IQueryable<UserReadDbModel> query,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var projection = await query
            .Where(user => user.Id == userId)
            .Select(user => new AuthenticatedUserProjection(
                user.Id,
                user.Email,
                user.ConfirmationStatus,
                user.Roles
                    .Select(userRole => userRole.Role!.Name)
                    .ToList(),
                user.Claims
                    .Select(claim => new AuthenticatedUserClaimReadModel(
                        claim.Type,
                        claim.Value))
                    .ToList(),
                user.Roles
                    .Select(userRole => new AuthenticatedUserRoleClaimsProjection(
                        userRole.Role!.Claims
                            .Select(claim => new AuthenticatedUserClaimReadModel(
                                claim.Type,
                                claim.Value))
                            .ToList()))
                    .ToList()))
            .FirstOrDefaultAsync(cancellationToken);

        if (projection is null)
        {
            return null;
        }

        return new AuthenticatedUserReadModel(
            projection.Id,
            projection.Email,
            projection.ConfirmationStatus,
            projection.Roles
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray(),
            projection.DirectClaims
                .Concat(projection.RoleClaims.SelectMany(role => role.Claims))
                .Distinct()
                .OrderBy(claim => claim.Type, StringComparer.Ordinal)
                .ThenBy(claim => claim.Value, StringComparer.Ordinal)
                .ToArray());
    }

    private sealed record AuthenticatedUserProjection(
        Guid Id,
        string Email,
        bool ConfirmationStatus,
        IReadOnlyCollection<string> Roles,
        IReadOnlyCollection<AuthenticatedUserClaimReadModel> DirectClaims,
        IReadOnlyCollection<AuthenticatedUserRoleClaimsProjection> RoleClaims);

    private sealed record AuthenticatedUserRoleClaimsProjection(
        IReadOnlyCollection<AuthenticatedUserClaimReadModel> Claims);
}
