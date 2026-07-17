using System.Security.Claims;

using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Management.GetDetails;
using PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Management.GetList;
using PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Management.GetStatuses;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Accounts.Read;

public sealed class AccountsReadRepository(IdentityReadDbContext dbContext)
    : IAccountsReadRepository
{
    public async Task<Result<PaginationResult<AccountListItemReadModel>>> GetPagedAsync(
        string? email,
        PaginationParameters pagination,
        CancellationToken cancellationToken)
    {
        var activeName = UserStatus.Active.Name;
        var activeDisplayName = UserStatus.Active.DisplayName;
        var blockedName = UserStatus.Blocked.Name;
        var blockedDisplayName = UserStatus.Blocked.DisplayName;
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
        var accounts = await query
            .Skip(pagination.Skip)
            .Take(pagination.Take)
            .Select(user => new AccountListItemReadModel(
                user.Id,
                user.Email!,
                user.UserName!,
                user.EmailConfirmed,
                user.Status,
                user.Status == activeName
                    ? activeDisplayName
                    : user.Status == blockedName
                        ? blockedDisplayName
                        : user.Status))
            .ToArrayAsync(cancellationToken);

        return Result.Success(
            PaginationResult<AccountListItemReadModel>.Create(
                accounts,
                pagination.PageNumber,
                pagination.PageSize,
                totalCount));
    }

    public async Task<Result<AccountDetailsReadModel>> GetDetailsByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var activeName = UserStatus.Active.Name;
        var activeDisplayName = UserStatus.Active.DisplayName;
        var blockedName = UserStatus.Blocked.Name;
        var blockedDisplayName = UserStatus.Blocked.DisplayName;
        var account = await dbContext.Set<UserReadDbModel>()
            .AsNoTracking()
            .Where(user => user.Id == id)
            .Select(user => new AccountDetailsReadModel(
                user.Id,
                user.Email!,
                user.UserName!,
                user.EmailConfirmed,
                user.Status,
                user.Status == activeName
                    ? activeDisplayName
                    : user.Status == blockedName
                        ? blockedDisplayName
                        : user.Status,
                user.Claims
                    .Select(claim => new Claim(
                        claim.ClaimType!,
                        claim.ClaimValue!))
                    .ToArray()))
            .SingleOrDefaultAsync(cancellationToken);

        return account is null
            ? Result.Failure<AccountDetailsReadModel>(
                Error.NotFound("Account was not found."))
            : Result.Success(account);
    }

    public Task<Result<IReadOnlyCollection<AccountStatusReadModel>>> GetStatusesAsync(
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyCollection<AccountStatusReadModel> statuses =
        [
            .. UserStatus.GetAll().Select(status =>
                new AccountStatusReadModel(
                    status.Id,
                    status.Name,
                    status.DisplayName))
        ];

        return Task.FromResult(Result.Success(statuses));
    }
}
