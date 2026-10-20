using System.Security.Claims;

using PANiXiDA.TacticalHeroes.Identity.Application.Claims;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Claims;

public sealed class ClaimCatalogValidatorTests
{
    [Theory(DisplayName = "Claim catalog validator should accept known or custom claims when claim is known or custom")]
    [InlineData("permission", "identity.roles.read")]
    [InlineData("PERMISSION", "compendium.heroes.update")]
    [InlineData(" permission ", " identity.users.read ")]
    [InlineData("custom", "arbitrary-value")]
    [InlineData("", "")]
    public void Validate_Should_ReturnSuccess_When_ClaimIsKnownOrCustom(string type, string value)
    {
        var validator = new ClaimCatalogValidator();
        var claim = new Claim(type, value);

        var result = validator.Validate(claim);

        result.IsValid.ShouldBeTrue();
    }

    [Theory(DisplayName = "Claim catalog validator should reject managed claims when managed claim is unknown")]
    [InlineData("permission", "unknown.resource.read")]
    [InlineData("permission", "identity")]
    [InlineData("permission", "identity.roles.read.own")]
    [InlineData("permission", "IDENTITY.ROLES.READ")]
    [InlineData("PERMISSION", "unknown")]
    [InlineData("permission", "")]
    [InlineData("section", "identity")]
    [InlineData("SECTION", "compendium")]
    [InlineData(" section ", " compendium ")]
    [InlineData("section", "unknown")]
    [InlineData("section", "identity.roles.read")]
    [InlineData("section", "Identity")]
    [InlineData("SECTION", "unknown")]
    [InlineData("section", "")]
    public void Validate_Should_ReturnValueError_When_ManagedClaimIsUnknown(string type, string value)
    {
        var validator = new ClaimCatalogValidator();
        var claim = new Claim(type, value);

        var result = validator.Validate(claim);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldHaveSingleItem().PropertyName.ShouldBe(nameof(Claim.Value));
    }
}
