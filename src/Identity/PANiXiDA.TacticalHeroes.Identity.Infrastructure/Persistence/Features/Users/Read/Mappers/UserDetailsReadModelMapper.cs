using System.Security.Claims;

using PANiXiDA.TacticalHeroes.Identity.Application.Users.GetDetails;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.Mappers;

internal sealed class UserDetailsReadModelMapper
    : IReadModelMapper<Guid, UserReadDbModel, UserDetailsReadModel>
{
    public static IQueryable<UserDetailsReadModel> ProjectTo(
        IQueryable<UserReadDbModel> query)
    {
        return query.Select(user => new UserDetailsReadModel(
            Id: user.Id,
            Email: user.Email!,
            UserName: user.UserName!,
            IsConfirmed: user.EmailConfirmed,
            Status: user.Status,
            StatusDisplayName: UserStatus.FromName(name: user.Status).DisplayName,
            Claims: user.Claims
                .Select(claim => new Claim(
                    type: claim.ClaimType!,
                    value: claim.ClaimValue!))
                .ToArray()));
    }
}
