using Microsoft.AspNetCore.Identity;

using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Services;

public sealed class PasswordHashingService : IPasswordHashingService
{
    private static readonly PasswordHashingUser User = new();

    private readonly PasswordHasher<PasswordHashingUser> _passwordHasher = new();

    public string HashPassword(string password)
    {
        return _passwordHasher.HashPassword(User, password);
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
