using System.Security.Cryptography;
using System.Text;

using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Services;

public sealed class OneTimeTokenService : IOneTimeTokenService
{
    private const int TokenByteLength = 32;

    public string GenerateToken()
    {
        return Base64UrlEncode(RandomNumberGenerator.GetBytes(TokenByteLength));
    }

    public string HashToken(string token)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));

        return Base64UrlEncode(hash);
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
