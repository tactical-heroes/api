using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Users.Common.Filters;
using PANiXiDA.TacticalHeroes.Identity.Application.Users.GetDetails;
using PANiXiDA.TacticalHeroes.Identity.Application.Users.GetList;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.GetDetails;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.GetList;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read;

public sealed class UsersReadRepository(IdentityReadDbContext dbContext) :
    EfReadRepository<IdentityReadDbContext, Guid, UserReadDbModel>(dbContext),
    IUsersReadRepository
{
    public Task<PaginationResult<UserListItemReadModel>> GetPageAsync(
        UsersFilter filter,
        PaginationParameters paginationParameters,
        SortingParameters sortingParameters,
        CancellationToken cancellationToken)
    {
        var query = ApplyFilter(
            query: Query,
            filter: filter);
        return GetPagedResultAsync<UserListItemReadModel, UserListItemReadModelMapper, UserListItemReadModelSorting>(
            query: query,
            paginationParameters: paginationParameters,
            sortingParameters: sortingParameters,
            cancellationToken: cancellationToken);
    }

    public Task<UserDetailsReadModel?> GetDetailsByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return GetByIdAsync<UserDetailsReadModel, UserDetailsReadModelMapper>(
            id: id,
            cancellationToken: cancellationToken);
    }

    private static IQueryable<UserReadDbModel> ApplyFilter(
        IQueryable<UserReadDbModel> query,
        UsersFilter filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.Email))
        {
            query = query.Where(user =>
                EF.Functions.ILike(
                    matchExpression: user.Email,
                    pattern: $"%{filter.Email.Trim()}%"));
        }

        return query;
    }
}
