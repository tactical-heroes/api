using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.GetUserInfo;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.GetUserInfo;

internal static class GetUserInfoMapper
{
    internal static GetUserInfoQuery ToQuery(Guid userId) =>
        new(UserId: userId);

    internal static GetUserInfoResponse ToResponse(
        UserInfoReadModel user,
        IEnumerable<string> scopes)
    {
        var scopeSet = new HashSet<string>(collection: scopes, comparer: StringComparer.Ordinal);

        return new GetUserInfoResponse(
            Subject: user.UserId.ToString(),
            Name: scopeSet.Contains(item: OpenIddictConstants.Scopes.Profile)
                ? user.Name
                : null,
            Email: scopeSet.Contains(item: OpenIddictConstants.Scopes.Email)
                ? user.Email
                : null,
            EmailVerified: scopeSet.Contains(item: OpenIddictConstants.Scopes.Email)
                ? user.EmailVerified
                : null,
            Role: scopeSet.Contains(item: OpenIddictConstants.Scopes.Roles) && user.Roles.Count > 0
                ? user.Roles
                : null);
    }
}
