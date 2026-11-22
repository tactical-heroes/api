namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Common;

public sealed record Claim(
    string Type,
    string Value)
{
    public static System.Security.Claims.Claim ToApplicationClaim(Claim claim)
    {
        return new System.Security.Claims.Claim(type: claim.Type, value: claim.Value);
    }

    public static Claim FromApplicationClaim(System.Security.Claims.Claim claim)
    {
        return new Claim(Type: claim.Type, Value: claim.Value);
    }
}
