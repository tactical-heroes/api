using PANiXiDA.TacticalHeroes.Identity.Domain.Users;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Domain.Users;

public sealed class UserRoleAssignmentsTests
{
    [Fact(DisplayName = "Role assignments should accept empty selections when roles are empty")]
    public void Create_Should_ReturnEmptySelection_When_RolesAreEmpty()
    {
        var result = UserRoleAssignments.Create([]);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEmpty();
    }

    [Fact(DisplayName = "Role assignments should reject a missing selection when roles are null")]
    public void Create_Should_ReturnValidationError_When_RolesAreNull()
    {
        var result = UserRoleAssignments.Create(null!);

        result.ShouldHaveSingleError(ErrorType.Validation, "Role IDs are required.")
            .ShouldHaveField(nameof(User.RoleIds));
    }

    [Fact(DisplayName = "Role assignments should preserve identifiers when roles are valid")]
    public void Create_Should_PreserveIdentifiers_When_RolesAreValid()
    {
        Guid[] roles = [Guid.CreateVersion7(), Guid.CreateVersion7()];

        var result = UserRoleAssignments.Create(roles);

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

        var result = UserRoleAssignments.Create(roles);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldHaveSingleItem().Type.ShouldBe(ErrorType.Validation);
    }
}
