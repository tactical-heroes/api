using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Entities.RoleClaims;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Domain.Roles.Entities.RoleClaims;

public sealed class RoleClaimIdTests
{
    [Fact(DisplayName = "Role claim id should create a non-empty value")]
    public void New_Should_CreateNonEmptyId_When_Called()
    {
        var id = RoleClaimId.New();

        id.Value.ShouldNotBe(Guid.Empty);
        id.ToString().ShouldBe(id.Value.ToString());
    }

    [Fact(DisplayName = "Role claim id should preserve a valid value")]
    public void Create_Should_ReturnId_When_ValueIsValid()
    {
        var value = Guid.CreateVersion7();

        var result = RoleClaimId.Create(value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(value);
    }

    [Fact(DisplayName = "Role claim id should reject an empty value")]
    public void Create_Should_ReturnValidationFailure_When_ValueIsEmpty()
    {
        var result = RoleClaimId.Create(Guid.Empty);

        result.ShouldHaveSingleError(ErrorType.Validation, "Role claim id cannot be empty.");
    }
}
