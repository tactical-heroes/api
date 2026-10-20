using System.Security.Claims;

using ApplicationClaimTypes = PANiXiDA.Core.Application.Authentication.ClaimTypes;
using PermissionCatalog = PANiXiDA.TacticalHeroes.Common.Authentication.Permissions.Permissions;

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
        if (string.Equals(type, ApplicationClaimTypes.Permission, StringComparison.OrdinalIgnoreCase))
        {
            return PermissionCatalog.All.Any(permission => permission.Code == value);
        }

        if (string.Equals(type, ApplicationClaimTypes.Section, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }
}
