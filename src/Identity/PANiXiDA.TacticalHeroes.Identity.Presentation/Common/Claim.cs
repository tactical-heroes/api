namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Common;

public sealed record Claim(
    string Type,
    string Value)
{
    public static System.Security.Claims.Claim ToApplicationClaim(Claim claim)
    {
        return new System.Security.Claims.Claim(claim.Type, claim.Value);
    }

    public static Claim FromApplicationClaim(System.Security.Claims.Claim claim)
    {
        return new Claim(claim.Type, claim.Value);
    }
}
