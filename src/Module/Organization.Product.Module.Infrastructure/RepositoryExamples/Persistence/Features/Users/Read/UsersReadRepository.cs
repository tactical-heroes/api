using Organization.Product.Module.Application.Users;
using Organization.Product.Module.Application.Users.Abstractions;
using Organization.Product.Module.Application.Users.GetDetails;
using Organization.Product.Module.Application.Users.GetList;
using Organization.Product.Module.Infrastructure.Persistence.Core;
using Organization.Product.Module.Infrastructure.Persistence.Features.Users.Read.Filters;
using Organization.Product.Module.Infrastructure.Persistence.Features.Users.Read.Mappers;

namespace Organization.Product.Module.Infrastructure.Persistence.Features.Users.Read;

public sealed class UsersReadRepository(TemplateReadDbContext dbContext) :
    EfReadRepository<TemplateReadDbContext, Guid, UserReadDbModel>(dbContext),
    IUsersReadRepository
{
    public Task<UserDetailsReadModel?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return GetByIdAsync<UserDetailsReadModel, UserDetailsReadModelMapper>(
            id,
            cancellationToken);
    }

    public Task<PaginationResult<UserListItemReadModel>> GetPagedListAsync(
        UsersFilterParameters filterParameters,
        PaginationParameters paginationParameters,
        SortParameters sortParameters,
        CancellationToken cancellationToken)
    {
        var query = Query;
        query = UsersFilter.Apply(query, filterParameters);

        return GetPagedResultAsync<UserListItemReadModel, UserListItemReadModelMapper>(
            query,
            paginationParameters,
            sortParameters,
            cancellationToken);
    }
}
