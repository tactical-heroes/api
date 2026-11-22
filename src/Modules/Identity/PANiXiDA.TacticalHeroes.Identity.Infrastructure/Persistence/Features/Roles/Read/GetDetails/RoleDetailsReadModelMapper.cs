using System.Security.Claims;

using PANiXiDA.TacticalHeroes.Modules.Identity.Application.Roles.GetDetails;
using PANiXiDA.TacticalHeroes.Modules.Identity.Infrastructure.Persistence.Features.Roles.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Infrastructure.Persistence.Features.Roles.Read.GetDetails;

internal sealed class RoleDetailsReadModelMapper
    : IReadModelMapper<Guid, RoleReadDbModel, RoleDetailsReadModel>
{
    public static IQueryable<RoleDetailsReadModel> ProjectTo(
        IQueryable<RoleReadDbModel> query)
    {
        return query.Select(role => new RoleDetailsReadModel(
            Id: role.Id,
            Name: role.Name!,
            IsDefault: role.IsDefault,
            Claims: role.Claims
                .Select(claim => new Claim(
                    type: claim.ClaimType!,
                    value: claim.ClaimValue!))
                .ToArray()));
    }
}
