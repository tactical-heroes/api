using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Common;

[Mapper]
internal static partial class ClaimMapper
{
    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    internal static partial System.Security.Claims.Claim ToApplicationClaim(
        Claim claim);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    internal static partial Claim FromApplicationClaim(
        System.Security.Claims.Claim claim);
}
