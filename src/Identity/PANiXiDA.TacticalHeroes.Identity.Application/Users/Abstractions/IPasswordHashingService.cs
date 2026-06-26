using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;

public interface IPasswordHashingService
{
    string HashPassword(ValidatedPassword password);

    bool VerifyPassword(
        PasswordHash passwordHash,
        string password);
}
