using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.GetUserInfo;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.GetUserInfo;

internal static class GetUserInfoMapper
{
    internal static GetUserInfoResponse ToResponse(
        UserInfoReadModel account,
        IEnumerable<string> scopes)
    {
        var scopeSet = new HashSet<string>(collection: scopes, comparer: StringComparer.Ordinal);

        return new GetUserInfoResponse(
            Subject: account.AccountId.ToString(),
            Name: scopeSet.Contains(OpenIddictConstants.Scopes.Profile)
                ? account.Name
                : null,
            Email: scopeSet.Contains(OpenIddictConstants.Scopes.Email)
                ? account.Email
                : null,
            EmailVerified: scopeSet.Contains(OpenIddictConstants.Scopes.Email)
                ? account.EmailVerified
                : null,
            Role: scopeSet.Contains(OpenIddictConstants.Scopes.Roles) && account.Roles.Count > 0
                ? account.Roles
                : null);
    }
}
