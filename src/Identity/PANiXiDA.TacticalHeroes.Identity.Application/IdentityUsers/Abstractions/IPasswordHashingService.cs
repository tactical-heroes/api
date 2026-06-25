using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityUsers.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Application.IdentityUsers.Abstractions;

public interface IPasswordHashingService
{
    PasswordHash HashPassword(ValidatedPassword password);

    bool VerifyPassword(
        PasswordHash passwordHash,
        string password);
}
