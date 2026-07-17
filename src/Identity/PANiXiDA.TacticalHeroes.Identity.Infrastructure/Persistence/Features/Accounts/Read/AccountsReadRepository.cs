using System.Security.Claims;

using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Management.GetDetails;
using PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Management.GetList;
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
                Id: user.Id,
                Email: user.Email!,
                UserName: user.UserName!,
                IsConfirmed: user.EmailConfirmed,
                Status: user.Status,
                StatusDisplayName: AccountStatus.FromName(name: user.Status).DisplayName))
            .ToArrayAsync(cancellationToken);

        return Result.Success(
            value: PaginationResult<AccountListItemReadModel>.Create(
                items: accounts,
                pageNumber: pagination.PageNumber,
                pageSize: pagination.PageSize,
                totalCount: totalCount));
    }

    public async Task<Result<AccountDetailsReadModel>> GetDetailsByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var account = await dbContext.Set<UserReadDbModel>()
            .AsNoTracking()
            .Where(user => user.Id == id)
            .Select(user => new AccountDetailsReadModel(
                Id: user.Id,
                Email: user.Email!,
                UserName: user.UserName!,
                IsConfirmed: user.EmailConfirmed,
                Status: user.Status,
                StatusDisplayName: AccountStatus.FromName(name: user.Status).DisplayName,
                Claims: user.Claims
                    .Select(claim => new Claim(
                        type: claim.ClaimType!,
                        value: claim.ClaimValue!))
                    .ToArray()))
            .SingleOrDefaultAsync(cancellationToken);

        return account is null
            ? Result.Failure<AccountDetailsReadModel>(
                error: Error.NotFound(message: "Account was not found."))
            : Result.Success(value: account);
    }
}
