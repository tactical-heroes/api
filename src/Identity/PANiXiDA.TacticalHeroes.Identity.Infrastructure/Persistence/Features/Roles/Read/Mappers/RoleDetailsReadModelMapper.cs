using System.Security.Claims;

using PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetDetails;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Read.Mappers;

internal sealed class RoleDetailsReadModelMapper
    : IReadModelMapper<Guid, RoleReadDbModel, RoleDetailsReadModel>
{
    public static IQueryable<RoleDetailsReadModel> ProjectTo(
        IQueryable<RoleReadDbModel> query)
    {
        return query.Select(role => new RoleDetailsReadModel(
            Id: role.Id,
            Name: role.Name!,
            Claims: role.Claims
                .Select(claim => new Claim(
                    type: claim.ClaimType!,
                    value: claim.ClaimValue!))
                .ToArray()));
    }
}
