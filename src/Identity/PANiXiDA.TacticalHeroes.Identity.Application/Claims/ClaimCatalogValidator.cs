using System.Security.Claims;

using PANiXiDA.TacticalHeroes.Common.Authentication;

using PermissionCatalog = PANiXiDA.TacticalHeroes.Common.Authentication.Permissions.Permissions;
using SectionCatalog = PANiXiDA.TacticalHeroes.Common.Authentication.Sections.Sections;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Claims;

public sealed class ClaimCatalogValidator : AbstractValidator<Claim>
{
    public ClaimCatalogValidator()
    {
        RuleFor(claim => claim.Value)
            .Must((claim, value) => IsKnownValue(claim.Type.Trim(), value.Trim()))
            .WithMessage("Claim value is not present in its catalog.");
    }

    private static bool IsKnownValue(string type, string value)
    {
        if (string.Equals(type, CustomClaimTypes.Permission, StringComparison.OrdinalIgnoreCase))
        {
            return PermissionCatalog.All.Any(permission => permission.Code == value);
        }

        if (string.Equals(type, CustomClaimTypes.Section, StringComparison.OrdinalIgnoreCase))
        {
            return SectionCatalog.All.Any(section => section.Code == value);
        }

        return true;
    }
}
