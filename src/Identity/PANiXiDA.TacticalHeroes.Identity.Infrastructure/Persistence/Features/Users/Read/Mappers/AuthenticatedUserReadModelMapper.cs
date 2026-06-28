using Riok.Mapperly.Abstractions;

using PANiXiDA.TacticalHeroes.Identity.Application.Users.GetAuthenticated;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.Mappers;

[Mapper]
internal sealed partial class AuthenticatedUserReadModelMapper :
    IReadModelMapper<Guid, UserReadDbModel, AuthenticatedUserReadModel>
{
    private const string RoleClaimsConstructorParameter = "roleClaims";

    public static partial IQueryable<AuthenticatedUserReadModel> ProjectTo(
        IQueryable<UserReadDbModel> query);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(nameof(UserReadDbModel.Roles), nameof(AuthenticatedUserReadModel.Roles), Use = nameof(MapRoles))]
    [MapProperty(nameof(UserReadDbModel.Claims), nameof(AuthenticatedUserReadModel.Claims), Use = nameof(MapDirectClaims))]
    [MapPropertyFromSource(RoleClaimsConstructorParameter, Use = nameof(MapRoleClaims))]
    private static partial AuthenticatedUserReadModel ToReadModel(
        UserReadDbModel source);

    private static IReadOnlyCollection<string> MapRoles(
        ICollection<UserRoleReadDbModel> source)
        => source
            .Where(userRole => userRole.Role != null)
            .Select(userRole => userRole.Role!.Name)
            .ToArray();

    private static IReadOnlyCollection<AuthenticatedUserClaimReadModel> MapDirectClaims(
        ICollection<UserClaimReadDbModel> source)
        => source
            .Select(claim => new AuthenticatedUserClaimReadModel(
                claim.Type,
                claim.Value))
            .ToArray();

    private static IReadOnlyCollection<AuthenticatedUserClaimReadModel> MapRoleClaims(
        UserReadDbModel source)
        => source.Roles
            .Where(userRole => userRole.Role != null)
            .SelectMany(userRole => userRole.Role!.Claims)
            .Select(claim => new AuthenticatedUserClaimReadModel(
                claim.Type,
                claim.Value))
            .ToArray();
}
