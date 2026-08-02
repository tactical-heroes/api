using Microsoft.EntityFrameworkCore;

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
            var pattern = $"%{EscapePattern(email.Trim())}%";
            query = query.Where(user =>
                user.Email != null &&
                EF.Functions.ILike(
                    user.Email,
                    pattern,
                    @"\"));
        }

        return query;
    }

    private static string EscapePattern(string value)
    {
        return value
            .Replace(@"\", @"\\", StringComparison.Ordinal)
            .Replace("%", @"\%", StringComparison.Ordinal)
            .Replace("_", @"\_", StringComparison.Ordinal);
    }
}
