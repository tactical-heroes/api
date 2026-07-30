using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.ExchangeToken;
using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.GetUserInfo;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Claims;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.OAuth;

public sealed class OAuthUsersRepository(IdentityReadDbContext dbContext)
    : IOAuthUsersRepository
{
    private IQueryable<UserReadDbModel> Query => dbContext.Set<UserReadDbModel>()
        .AsNoTracking()
        .Include(user => user.Claims)
        .Include(user => user.Roles)
            .ThenInclude(userRole => userRole.Role)
                .ThenInclude(role => role!.Claims)
        .AsSingleQuery();

    public async Task<Result<ExchangeTokenReadModel>> GetExchangeTokenByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var applicationUser = await Query
            .SingleOrDefaultAsync(user => user.Id == userId, cancellationToken);

        if (applicationUser is null)
        {
            return Result.Failure<ExchangeTokenReadModel>(
                error: Error.NotFound(message: "User was not found."));
        }

        var availabilityResult = EnsureAvailable(applicationUser);

        return availabilityResult.IsFailure
            ? Result.Failure<ExchangeTokenReadModel>(errors: availabilityResult.Errors)
            : Result.Success(
                value: new ExchangeTokenReadModel(
                    Claims: IdentityClaimsFactory.Create(user: applicationUser)));
    }

    public async Task<Result<UserInfoReadModel>> GetUserInfoByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var applicationUser = await Query
            .SingleOrDefaultAsync(user => user.Id == userId, cancellationToken);

        if (applicationUser is null)
        {
            return Result.Failure<UserInfoReadModel>(
                error: Error.NotFound(message: "User was not found."));
        }

        var availabilityResult = EnsureAvailable(applicationUser);

        if (availabilityResult.IsFailure)
        {
            return Result.Failure<UserInfoReadModel>(errors: availabilityResult.Errors);
        }

        IReadOnlyCollection<string> roles =
        [
            .. applicationUser.Roles
                .Select(userRole => userRole.Role?.Name)
                .Where(roleName => !string.IsNullOrWhiteSpace(roleName))
                .Select(roleName => roleName!)
                .Distinct(StringComparer.Ordinal)
        ];

        return Result.Success(
            value: new UserInfoReadModel(
                UserId: applicationUser.Id,
                Name: applicationUser.UserName,
                Email: applicationUser.Email,
                EmailVerified: applicationUser.EmailConfirmed,
                Roles: roles));
    }

    private static Result EnsureAvailable(UserReadDbModel applicationUser)
    {
        if (string.Equals(
                applicationUser.Status,
                UserStatus.Blocked.Name,
                StringComparison.Ordinal))
        {
            return Result.Failure(error: Error.Forbidden(message: "User is blocked."));
        }

        return applicationUser.EmailConfirmed
            ? Result.Success()
            : Result.Failure(error: Error.Forbidden(message: "User is not confirmed."));
    }
}
