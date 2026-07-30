using System.Security.Claims;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Claims;

internal sealed class IdentityClaimComparer : IEqualityComparer<Claim>
{
    public static readonly IdentityClaimComparer Instance = new();

    private IdentityClaimComparer()
    {
    }

    public bool Equals(
        Claim? first,
        Claim? second)
    {
        if (ReferenceEquals(first, second))
        {
            return true;
        }

        return first is not null &&
            second is not null &&
            first.Type == second.Type &&
            first.Value == second.Value;
    }

    public int GetHashCode(Claim claim)
    {
        return HashCode.Combine(claim.Type, claim.Value);
    }
}
