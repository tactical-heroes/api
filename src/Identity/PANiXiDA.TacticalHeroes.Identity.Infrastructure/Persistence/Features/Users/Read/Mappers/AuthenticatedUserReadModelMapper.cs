using Riok.Mapperly.Abstractions;

using PANiXiDA.TacticalHeroes.Identity.Application.Users.GetAuthenticated;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.Mappers;

[Mapper]
internal sealed partial class AuthenticatedUserReadModelMapper :
    IReadModelMapper<Guid, UserReadDbModel, AuthenticatedUserReadModel>
{
    public static IQueryable<AuthenticatedUserReadModel> ProjectTo(
        IQueryable<UserReadDbModel> query)
    {
        return query.Select(user => new AuthenticatedUserReadModel(
            user.Id,
            user.Email,
            user.ConfirmationStatus,
            user.Roles
                .Where(userRole => userRole.Role != null)
                .Select(userRole => userRole.Role!.Name)
                .ToArray(),
            user.Claims
                .Select(claim => new AuthenticatedUserClaimReadModel(
                    claim.Type,
                    claim.Value))
                .ToArray(),
            user.Roles
                .Where(userRole => userRole.Role != null)
                .SelectMany(userRole => userRole.Role!.Claims)
                .Select(claim => new AuthenticatedUserClaimReadModel(
                    claim.Type,
                    claim.Value))
                .ToArray()));
    }

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(nameof(UserReadDbModel.Roles), nameof(AuthenticatedUserReadModel.Roles), Use = nameof(MapRoles))]
    [MapPropertyFromSource(nameof(AuthenticatedUserReadModel.Claims), Use = nameof(MapClaims))]
    private static partial AuthenticatedUserReadModel ToReadModel(
        UserReadDbModel source);

    private static IReadOnlyCollection<string> MapRoles(
        ICollection<UserRoleReadDbModel> source)
    {
        return source
            .Where(userRole => userRole.Role != null)
            .Select(userRole => userRole.Role!.Name)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(roleName => roleName, StringComparer.Ordinal)
            .ToArray();
    }

    private static IReadOnlyCollection<AuthenticatedUserClaimReadModel> MapClaims(
        UserReadDbModel source)
    {
        return source.Claims
            .Select(claim => new AuthenticatedUserClaimReadModel(
                claim.Type,
                claim.Value))
            .Concat(
                source.Roles
                    .Where(userRole => userRole.Role != null)
                    .SelectMany(userRole => userRole.Role!.Claims)
                    .Select(claim => new AuthenticatedUserClaimReadModel(
                        claim.Type,
                        claim.Value)))
            .DistinctBy(claim => new
            {
                claim.Type,
                claim.Value
            })
            .OrderBy(claim => claim.Type, StringComparer.Ordinal)
            .ThenBy(claim => claim.Value, StringComparer.Ordinal)
            .ToArray();
    }
}
