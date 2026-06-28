using PANiXiDA.TacticalHeroes.Identity.Application.Users.GetAuthenticated;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.Mappers;

internal sealed class AuthenticatedUserReadModelMapper
    : IReadModelMapper<Guid, UserReadDbModel, AuthenticatedUserReadModel>
{
    public static IQueryable<AuthenticatedUserReadModel> ProjectTo(
        IQueryable<UserReadDbModel> query)
    {
        return query.Select(user => new AuthenticatedUserReadModel(
            user.Id,
            user.Email,
            user.ConfirmationStatus,
            user.Roles
                .Select(userRole => new AuthenticatedUserAssignedRoleReadModel(
                    userRole.Role == null
                        ? null
                        : new AuthenticatedUserRoleReadModel(
                            userRole.Role.Name,
                            userRole.Role.Claims
                                .Select(claim => new AuthenticatedUserClaimReadModel(
                                    claim.Type,
                                    claim.Value))
                                .ToList())))
                .ToList(),
            user.Claims
                .Select(claim => new AuthenticatedUserClaimReadModel(
                    claim.Type,
                    claim.Value))
                .ToList()));
    }
}
