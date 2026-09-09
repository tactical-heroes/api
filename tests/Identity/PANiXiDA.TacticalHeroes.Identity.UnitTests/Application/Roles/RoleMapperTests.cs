using PANiXiDA.TacticalHeroes.Identity.Application.Roles;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Roles;

public sealed class RoleMapperTests
{
    [Fact(DisplayName = "Role mapper should collect invalid identity fields when identity is invalid")]
    public void ToDomain_Should_ReturnValidationFailures_When_IdentityIsInvalid()
    {
        var result = RoleMapper.ToDomain(Guid.Empty, string.Empty, []);

        result.IsFailure.ShouldBeTrue();
        result.Errors.Count.ShouldBe(2);
    }

    [Fact(DisplayName = "Role mapper should reject invalid claims when claim is invalid")]
    public void ToDomain_Should_ReturnValidationFailures_When_ClaimIsInvalid()
    {
        var result = RoleMapper.ToDomain(Guid.CreateVersion7(), "admin", [("", "")]);

        result.IsFailure.ShouldBeTrue();
        result.Errors.Count.ShouldBe(2);
    }

    [Fact(DisplayName = "Role mapper should normalize and deduplicate claims when values are valid")]
    public void ToDomain_Should_NormalizeAndDeduplicateClaims_When_ValuesAreValid()
    {
        var result = RoleMapper.ToDomain(
            Guid.CreateVersion7(), " ADMIN ",
            [(" permission ", " heroes.manage "), ("permission", "heroes.manage")]);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Name.Value.ShouldBe("admin");
        var claim = result.Value.Claims.ShouldHaveSingleItem();
        claim.Type.Value.ShouldBe("permission");
        claim.Value.Value.ShouldBe("heroes.manage");
    }
}
