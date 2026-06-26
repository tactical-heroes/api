namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;

public interface IUserTokenService
{
    string GenerateToken();

    string HashToken(string token);
}
