using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityUsers.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Application.IdentityUsers.Abstractions;

public interface IIdentityTokenService
{
    string GenerateToken();

    TokenHash HashToken(string token);
}
