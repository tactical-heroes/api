namespace PANiXiDA.TacticalHeroes.Modules.Identity.Application.Auth.Abstractions;

public interface ICurrentUserSessionRefresh
{
    void RequireRefresh(string authorizationId);
}
