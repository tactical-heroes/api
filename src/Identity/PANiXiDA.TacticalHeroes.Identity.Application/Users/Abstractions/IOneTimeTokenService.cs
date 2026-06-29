namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;

public interface IOneTimeTokenService
{
    string GenerateToken();

    string HashToken(string token);
}
