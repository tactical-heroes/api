using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.GetUserInfo;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.GetUserInfo;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal static partial class GetUserInfoMapper
{
    internal static partial GetUserInfoQuery ToQuery(Guid userId);

    [MapperIgnore]
    internal static GetUserInfoResponse ToResponse(
        UserInfoReadModel user,
        IEnumerable<string> scopes)
    {
        return ToResponse(source: new GetUserInfoResponseSource(
            user: user,
            scopes: scopes));
    }

    private static partial GetUserInfoResponse ToResponse(
        GetUserInfoResponseSource source);

    private sealed class GetUserInfoResponseSource
    {
        private readonly UserInfoReadModel _user;
        private readonly HashSet<string> _scopes;

        internal GetUserInfoResponseSource(
            UserInfoReadModel user,
            IEnumerable<string> scopes)
        {
            _user = user;
            _scopes = new HashSet<string>(
                collection: scopes,
                comparer: StringComparer.Ordinal);
        }

        public string Subject => _user.UserId.ToString();

        public string? Name =>
            _scopes.Contains(OpenIddictConstants.Scopes.Profile)
                ? _user.Name
                : null;

        public string? Email =>
            _scopes.Contains(OpenIddictConstants.Scopes.Email)
                ? _user.Email
                : null;

        public bool? EmailVerified =>
            _scopes.Contains(OpenIddictConstants.Scopes.Email)
                ? _user.EmailVerified
                : null;

        public IReadOnlyCollection<string>? Role =>
            _scopes.Contains(OpenIddictConstants.Scopes.Roles) &&
            _user.Roles.Count > 0
                ? _user.Roles
                : null;
    }
}
