using System.Security.Cryptography;
using System.Text;

using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Users;

public sealed class UserTokenService : IUserTokenService
{
    private const int TokenByteLength = 32;

    public string GenerateToken()
    {
        return Base64UrlEncode(RandomNumberGenerator.GetBytes(TokenByteLength));
    }

    public TokenHash HashToken(string token)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));

        return TokenHash.Create(Base64UrlEncode(hash)).Value;
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
