using Microsoft.AspNetCore.Identity;

using PANiXiDA.TacticalHeroes.Identity.Application.Users;
using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Users;

public sealed class PasswordHashingService : IPasswordHashingService
{
    private static readonly PasswordHashingUser User = new();

    private readonly PasswordHasher<PasswordHashingUser> _passwordHasher = new();

    public PasswordHash HashPassword(ValidatedPassword password)
    {
        var hash = _passwordHasher.HashPassword(User, password.Value);

        return PasswordHash.Create(hash).Value;
    }

    public bool VerifyPassword(
        PasswordHash passwordHash,
        string password)
    {
        var result = _passwordHasher.VerifyHashedPassword(User, passwordHash.Value, password);

        return result is PasswordVerificationResult.Success or
            PasswordVerificationResult.SuccessRehashNeeded;
    }

    private sealed class PasswordHashingUser;
}
