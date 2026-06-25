using Microsoft.EntityFrameworkCore;

using Organization.Product.Module.Application.Users;

namespace Organization.Product.Module.Infrastructure.Persistence.Features.Users.Read.Filters;

internal static class UsersFilter
{
    public static IQueryable<UserReadDbModel> Apply(
        IQueryable<UserReadDbModel> dbObjects,
        UsersFilterParameters filterParameters)
    {
        if (!string.IsNullOrWhiteSpace(filterParameters.Role))
        {
            dbObjects = dbObjects.Where(item
                => EF.Functions.ILike(item.Role, filterParameters.Role));
        }

        return dbObjects;
    }
}
