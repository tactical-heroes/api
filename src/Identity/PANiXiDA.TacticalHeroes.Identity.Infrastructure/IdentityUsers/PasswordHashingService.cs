using Microsoft.AspNetCore.Identity;

using PANiXiDA.TacticalHeroes.Identity.Application.IdentityUsers;
using PANiXiDA.TacticalHeroes.Identity.Application.IdentityUsers.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityUsers.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityUsers;

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
