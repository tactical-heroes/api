using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Users.GetDetails;
using PANiXiDA.TacticalHeroes.Identity.Application.Users.GetList;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.Filters;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.Mappers;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read;

public sealed class UsersReadRepository(IdentityReadDbContext dbContext) :
    EfReadRepository<IdentityReadDbContext, Guid, UserReadDbModel>(dbContext),
    IUsersReadRepository
{
    private static readonly SortParameters Sort = new(
        Field: nameof(UserReadDbModel.Email),
        Order: SortOrder.Ascending);

    public Task<PaginationResult<UserListItemReadModel>> GetPagedAsync(
        string? email,
        PaginationParameters pagination,
        CancellationToken cancellationToken)
    {
        var query = UsersFilter.Apply(
            query: Query,
            email: email);
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
}
