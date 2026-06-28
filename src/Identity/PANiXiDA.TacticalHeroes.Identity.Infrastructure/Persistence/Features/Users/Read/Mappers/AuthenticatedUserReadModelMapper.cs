using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Identity.Application.Users.GetAuthenticated;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.DbModels;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal sealed partial class AuthenticatedUserReadModelMapper
    : IReadModelMapper<Guid, UserReadDbModel, AuthenticatedUserReadModel>
{
    public static partial IQueryable<AuthenticatedUserReadModel> ProjectTo(
        IQueryable<UserReadDbModel> query);

    public static async Task<AuthenticatedUserReadModel?> GetByIdAsync(
        IQueryable<UserReadDbModel> query,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var user = await ProjectTo(query.Where(readUser => readUser.Id == userId))
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return null;
        }

        var roleClaims = await query
            .Where(readUser => readUser.Id == userId)
            .SelectMany(readUser => readUser.Roles
                .SelectMany(userRole => userRole.Role!.Claims
                    .Select(claim => new AuthenticatedUserClaimReadModel(
                        claim.Type,
                        claim.Value))))
            .ToArrayAsync(cancellationToken);

        return user with
        {
            Roles =
            [
                .. user.Roles
                    .Distinct(StringComparer.Ordinal)
                    .Order(StringComparer.Ordinal)
            ],
            Claims =
            [
                .. user.Claims
                    .Concat(roleClaims)
                    .Distinct()
                    .OrderBy(claim => claim.Type, StringComparer.Ordinal)
                    .ThenBy(claim => claim.Value, StringComparer.Ordinal)
            ]
        };
    }

    [MapProperty(nameof(UserReadDbModel.Roles), nameof(AuthenticatedUserReadModel.Roles), Use = nameof(MapRoles))]
    private static partial AuthenticatedUserReadModel Map(
        UserReadDbModel user);

    [UserMapping(Default = false)]
    private static IReadOnlyCollection<string> MapRoles(
        ICollection<UserRoleReadDbModel> roles)
        => roles
            .Select(role => role.Role!.Name)
            .ToArray();
}
