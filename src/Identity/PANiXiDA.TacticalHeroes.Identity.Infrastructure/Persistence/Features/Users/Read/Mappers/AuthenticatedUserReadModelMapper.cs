using PANiXiDA.TacticalHeroes.Identity.Application.Users.Authentication;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.Mappers;

internal sealed class AuthenticatedUserReadModelMapper :
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
}
