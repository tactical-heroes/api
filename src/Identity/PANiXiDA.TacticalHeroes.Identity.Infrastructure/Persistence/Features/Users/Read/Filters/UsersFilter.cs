using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Read.Filters;

internal static class UsersFilter
{
    public static IQueryable<UserReadDbModel> Apply(
        IQueryable<UserReadDbModel> query,
        string? email)
    {
        if (!string.IsNullOrWhiteSpace(email))
        {
            query = query.Where(user => user.Email == email.Trim());
        }

        return query;
    }
}
