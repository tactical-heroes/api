using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Domain.Roles;

public sealed class RoleIdTests
{
    [Fact(DisplayName = "Role id should create a non-empty value when called")]
    public void New_Should_CreateNonEmptyId_When_Called()
    {
        var id = RoleId.New();

        id.Value.ShouldNotBe(Guid.Empty);
        id.ToString().ShouldBe(id.Value.ToString());
    }

    [Fact(DisplayName = "Role id should preserve a valid value when value is valid")]
    public void Create_Should_ReturnId_When_ValueIsValid()
    {
        var value = Guid.CreateVersion7();

        var result = RoleId.Create(value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(value);
    }

    [Fact(DisplayName = "Role id should reject an empty value when value is empty")]
    public void Create_Should_ReturnValidationFailure_When_ValueIsEmpty()
    {
        var result = RoleId.Create(Guid.Empty);

        result.ShouldHaveSingleError(ErrorType.Validation, "Role id cannot be empty.");
    }

    [Fact(DisplayName = "Role id should return its value when converted to string")]
    public void ToString_Should_ReturnValue_When_ConvertedToString()
    {
        var value = Guid.CreateVersion7();
        var id = RoleId.Create(value).Value;

        var result = id.ToString();

        result.ShouldBe(value.ToString());
    }
}
