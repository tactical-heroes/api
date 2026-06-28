using Riok.Mapperly.Abstractions;

using PANiXiDA.TacticalHeroes.Identity.Application.Users.GetAuthenticated;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.Mappers;

[Mapper]
internal static partial class AuthenticatedUserReadModelMapper
{
    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(nameof(UserReadDbModel.Roles), nameof(AuthenticatedUserReadModel.Roles), Use = nameof(MapRoles))]
    [MapPropertyFromSource(nameof(AuthenticatedUserReadModel.Claims), Use = nameof(MapClaims))]
    public static partial AuthenticatedUserReadModel ToReadModel(
        this UserReadDbModel source);

    private static IReadOnlyCollection<string> MapRoles(
        ICollection<UserRoleReadDbModel> source)
    {
        return source
            .Where(userRole => userRole.Role is not null)
            .Select(userRole => userRole.Role!.Name)
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
                    .Where(userRole => userRole.Role is not null)
                    .SelectMany(userRole => userRole.Role!.Claims)
                    .Select(claim => new AuthenticatedUserClaimReadModel(
                        claim.Type,
                        claim.Value)))
            .DistinctBy(claim => new
            {
                claim.Type,
                claim.Value
            })
            .ToArray();
    }
}
