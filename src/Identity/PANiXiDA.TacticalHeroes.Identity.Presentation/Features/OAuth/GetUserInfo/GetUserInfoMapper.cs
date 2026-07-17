using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.GetUserInfo;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.GetUserInfo;

internal static class GetUserInfoMapper
{
    internal static GetUserInfoResponse ToResponse(
        UserInfoReadModel account,
        IEnumerable<string> scopes)
    {
        var scopeSet = new HashSet<string>(scopes, StringComparer.Ordinal);

        return new GetUserInfoResponse(
            account.AccountId.ToString(),
            scopeSet.Contains(OpenIddictConstants.Scopes.Profile)
                ? account.Name
                : null,
            scopeSet.Contains(OpenIddictConstants.Scopes.Email)
                ? account.Email
                : null,
            scopeSet.Contains(OpenIddictConstants.Scopes.Email)
                ? account.EmailVerified
                : null,
            scopeSet.Contains(OpenIddictConstants.Scopes.Roles) && account.Roles.Count > 0
                ? account.Roles
                : null);
    }
}
