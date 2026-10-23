using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Common;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Claims;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Auth;

public sealed class AuthenticatedUserReader(IdentityReadDbContext dbContext, IOptions<IdentityOptions> identityOptions)
    : IAuthenticatedUserReader
{
    public async Task<Result<AuthenticatedUserReadModel>> GetByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await dbContext.Set<UserReadDbModel>()
            .AsNoTracking()
            .Include(user => user.Claims)
            .Include(user => user.Roles)
            .ThenInclude(userRole => userRole.Role)
            .ThenInclude(role => role!.Claims)
            .AsSingleQuery()
            .SingleOrDefaultAsync(user => user.Id == userId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<AuthenticatedUserReadModel>(Error.NotFound(message: "User was not found."));
        }

        if (string.Equals(user.Status, UserStatus.Blocked.Name, StringComparison.Ordinal))
        {
            return Result.Failure<AuthenticatedUserReadModel>(Error.Forbidden(message: "User is blocked."));
        }

        if (!user.EmailConfirmed)
        {
            return Result.Failure<AuthenticatedUserReadModel>(Error.Forbidden(message: "User is not confirmed."));
        }

        return Result.Success(new AuthenticatedUserReadModel(
            user.Id, user.Email, user.UserName, IdentityClaimsFactory.Create(user, identityOptions.Value)));
    }
}
