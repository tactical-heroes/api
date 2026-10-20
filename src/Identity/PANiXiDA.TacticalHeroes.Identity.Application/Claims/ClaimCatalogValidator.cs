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
        return string.Equals(type, ApplicationClaimTypes.Permission, StringComparison.OrdinalIgnoreCase)
            && PermissionCatalog.All.Any(permission => permission.Code == value);
    }
}
