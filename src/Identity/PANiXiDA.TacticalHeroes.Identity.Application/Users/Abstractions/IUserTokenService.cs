using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;

public interface IUserTokenService
{
    string GenerateToken();

    TokenHash HashToken(string token);
}
