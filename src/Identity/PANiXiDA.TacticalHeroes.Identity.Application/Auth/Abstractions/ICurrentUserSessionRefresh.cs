namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;

public interface ICurrentUserSessionRefresh
{
    void RequireRefresh(string authorizationId);
}
