using System.Security.Claims;

using PANiXiDA.TacticalHeroes.Identity.Application.Claims;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Claims;

public sealed class ClaimCatalogValidatorTests
{
    [Theory(DisplayName = "Claim catalog validator should accept permissions when permission is known")]
    [InlineData("permission", "identity.roles.read")]
    [InlineData("PERMISSION", "compendium.heroes.update")]
    [InlineData(" permission ", " identity.users.read ")]
    public void Validate_Should_ReturnSuccess_When_PermissionIsKnown(string type, string value)
    {
        var validator = new ClaimCatalogValidator();
        var claim = new Claim(type, value);

        var result = validator.Validate(claim);

        result.IsValid.ShouldBeTrue();
    }

    [Theory(DisplayName = "Claim catalog validator should reject claims when type or permission is unknown")]
    [InlineData("custom", "arbitrary-value")]
    [InlineData("custom", "identity.roles.read")]
    [InlineData("", "")]
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
    public void Validate_Should_ReturnValueError_When_TypeOrPermissionIsUnknown(string type, string value)
    {
        var validator = new ClaimCatalogValidator();
        var claim = new Claim(type, value);

        var result = validator.Validate(claim);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldHaveSingleItem().PropertyName.ShouldBe(nameof(Claim.Value));
    }
}
