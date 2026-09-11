using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Users.Common.Filters;
using PANiXiDA.TacticalHeroes.Identity.Application.Users.GetDetails;
using PANiXiDA.TacticalHeroes.Identity.Application.Users.GetList;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.Mappers;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read;

public sealed class UsersReadRepository(IdentityReadDbContext dbContext) :
    EfReadRepository<IdentityReadDbContext, Guid, UserReadDbModel>(dbContext),
    IUsersReadRepository
{
    private static readonly SortParameters Sort = new(
        Field: nameof(UserReadDbModel.Email),
        Order: SortOrder.Ascending);

    public Task<PaginationResult<UserListItemReadModel>> GetPageAsync(
        UsersFilter filter,
        PaginationParameters pagination,
        CancellationToken cancellationToken)
    {
        var query = ApplyFilter(
            query: Query,
            filter: filter);
        return GetPagedResultAsync<UserListItemReadModel, UserListItemReadModelMapper>(
            query: query,
            paginationParameters: pagination,
            sortParameters: Sort,
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
