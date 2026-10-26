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

    [Fact(DisplayName = "Role assignments should accept empty selections when roles are empty")]
    public void Create_Should_ReturnEmptyCollection_When_RolesAreEmpty()
    {
        var result = RoleId.Create([]);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEmpty();
    }

    [Fact(DisplayName = "Role assignments should reject a missing selection when values are null")]
    public void Create_Should_ReturnValidationError_When_ValuesAreNull()
    {
        var result = RoleId.Create(null!);

        result.ShouldHaveSingleError(ErrorType.Validation, "Role IDs are required.");
    }

    [Fact(DisplayName = "Role assignments should preserve identifiers when roles are valid")]
    public void Create_Should_ReturnIds_When_RolesAreValid()
    {
        Guid[] roles = [Guid.CreateVersion7(), Guid.CreateVersion7()];

        var result = RoleId.Create(roles);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Select(role => role.Value).ShouldBe(roles);
    }

    [Theory(DisplayName = "Role assignments should reject invalid selections when roles are invalid")]
    [InlineData(false)]
    [InlineData(true)]
    public void Create_Should_ReturnValidationError_When_RolesAreInvalid(bool duplicate)
    {
        var id = Guid.CreateVersion7();
        Guid[] roles = duplicate ? [id, id] : [Guid.Empty];

        var result = RoleId.Create(roles);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldHaveSingleItem().Type.ShouldBe(ErrorType.Validation);
    }
}
