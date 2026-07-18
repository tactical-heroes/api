using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Domain.Roles;

public sealed class RoleIdTests
{
    [Fact(DisplayName = "Role id should create a non-empty value")]
    public void New_Should_CreateNonEmptyId_When_Called()
    {
        var id = RoleId.New();

        id.Value.ShouldNotBe(Guid.Empty);
        id.ToString().ShouldBe(id.Value.ToString());
    }

    [Fact(DisplayName = "Role id should preserve a valid value")]
    public void Create_Should_ReturnId_When_ValueIsValid()
    {
        var value = Guid.CreateVersion7();

        var result = RoleId.Create(value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(value);
    }

    [Fact(DisplayName = "Role id should reject an empty value")]
    public void Create_Should_ReturnValidationFailure_When_ValueIsEmpty()
    {
        var result = RoleId.Create(Guid.Empty);

        result.ShouldHaveSingleError(ErrorType.Validation, "Role id cannot be empty.");
    }
}
