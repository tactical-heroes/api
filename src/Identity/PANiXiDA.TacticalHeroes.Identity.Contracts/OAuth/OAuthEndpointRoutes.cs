namespace PANiXiDA.TacticalHeroes.Identity.Contracts.OAuth;

public static class OAuthEndpointRoutes
{
    public const string Group = "connect";

    public const string PushedAuthorization = "/par";

    public const string Authorization = "/authorize";

    public const string Token = "/token";

    public const string UserInfo = "/userinfo";

    public const string EndSession = "/logout";

    public const string Introspection = "/introspect";

    public const string Revocation = "/revoke";

    public static string GetPath(string endpointRoute)
    {
        return string.Concat('/', Group, endpointRoute);
    }
}
