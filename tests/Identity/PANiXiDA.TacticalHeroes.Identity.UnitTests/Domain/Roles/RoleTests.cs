using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Domain.Roles;

public sealed class RoleTests
{
    [Fact(DisplayName = "Create should restore persisted role state")]
    public void Create_Should_RestoreState_When_PersistedValuesAreValid()
    {
        var id = Guid.CreateVersion7();

        var result = Role.Create(
            id,
            " ADMIN ",
            [("permission", "heroes.manage")]);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.Value.ShouldBe(id);
        result.Value.Name.Value.ShouldBe("admin");
        var claim = result.Value.Claims.ShouldHaveSingleItem();
        claim.Type.Value.ShouldBe("permission");
        claim.Value.Value.ShouldBe("heroes.manage");
    }

    [Fact(DisplayName = "Create should reject invalid persisted state")]
    public void Create_Should_ReturnValidationFailures_When_PersistedStateIsInvalid()
    {
        var result = Role.Create(Guid.Empty, "", []);

        result.IsFailure.ShouldBeTrue();
        result.Errors.Count.ShouldBe(2);
    }

    [Fact(DisplayName = "Grant claim should add a valid claim only once")]
    public void GrantClaim_Should_AddClaimOnce_When_ClaimIsValid()
    {
        var role = CreateRole();

        var firstResult = role.GrantClaim("permission", "heroes.manage");
        var secondResult = role.GrantClaim("permission", "heroes.manage");

        firstResult.IsSuccess.ShouldBeTrue();
        secondResult.IsSuccess.ShouldBeTrue();
        role.Claims.ShouldHaveSingleItem();
    }

    [Fact(DisplayName = "Grant claim should reject an invalid claim")]
    public void GrantClaim_Should_ReturnValidationFailure_When_ClaimIsInvalid()
    {
        var role = CreateRole();

        var result = role.GrantClaim("", "heroes.manage");

        result.ShouldHaveSingleError(ErrorType.Validation, "Claim type cannot be empty.");
        role.Claims.ShouldBeEmpty();
    }

    [Fact(DisplayName = "Revoke claim should remove the matching claim")]
    public void RevokeClaim_Should_RemoveClaim_When_ClaimExists()
    {
        var role = CreateRole();
        role.GrantClaim("permission", "heroes.read");
        role.GrantClaim("permission", "heroes.manage");

        var result = role.RevokeClaim("permission", "heroes.read");

        result.IsSuccess.ShouldBeTrue();
        var claim = role.Claims.ShouldHaveSingleItem();
        claim.Value.Value.ShouldBe("heroes.manage");
    }

    private static Role CreateRole()
    {
        return Role.Create(Guid.CreateVersion7(), "admin", []).Value;
    }
}
