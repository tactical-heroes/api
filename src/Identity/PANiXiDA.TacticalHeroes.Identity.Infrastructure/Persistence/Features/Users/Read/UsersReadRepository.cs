using System.Security.Claims;

using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Users.GetDetails;
using PANiXiDA.TacticalHeroes.Identity.Application.Users.GetList;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read;

public sealed class UsersReadRepository(IdentityReadDbContext dbContext)
    : IUsersReadRepository
{
    public async Task<Result<PaginationResult<UserListItemReadModel>>> GetPagedAsync(
        string? email,
        PaginationParameters pagination,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Set<UserReadDbModel>()
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(email))
        {
            query = query.Where(user => user.Email == email.Trim());
        }

        query = query
            .OrderBy(user => user.Email)
            .ThenBy(user => user.Id);

        var totalCount = await query.LongCountAsync(cancellationToken);
        var users = await query
            .Skip(pagination.Skip)
            .Take(pagination.Take)
            .Select(user => new UserListItemReadModel(
                Id: user.Id,
                Email: user.Email!,
                UserName: user.UserName!,
                IsConfirmed: user.EmailConfirmed,
                Status: user.Status,
                StatusDisplayName: UserStatus.FromName(name: user.Status).DisplayName))
            .ToArrayAsync(cancellationToken);

        return Result.Success(
            value: PaginationResult<UserListItemReadModel>.Create(
                items: users,
                pageNumber: pagination.PageNumber,
                pageSize: pagination.PageSize,
                totalCount: totalCount));
    }

    public async Task<Result<UserDetailsReadModel>> GetDetailsByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var user = await dbContext.Set<UserReadDbModel>()
            .AsNoTracking()
            .Where(user => user.Id == id)
            .Select(user => new UserDetailsReadModel(
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
                    .ToArray()))
            .SingleOrDefaultAsync(cancellationToken);

        return user is null
            ? Result.Failure<UserDetailsReadModel>(
                error: Error.NotFound(message: "User was not found."))
            : Result.Success(value: user);
    }
}
