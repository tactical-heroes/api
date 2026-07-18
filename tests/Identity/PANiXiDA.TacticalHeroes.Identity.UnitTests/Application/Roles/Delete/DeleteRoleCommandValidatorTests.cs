using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Delete;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Roles.Delete;

public sealed class DeleteRoleCommandValidatorTests
{
    [Fact(DisplayName = "Delete role validator should reject an empty role id")]
    public void Validate_Should_ReturnError_When_RoleIdIsEmpty()
    {
        var validator = new DeleteRoleCommandValidator();

        var result = validator.Validate(new DeleteRoleCommand(Guid.Empty));

        result.Errors.ShouldContain(error => error.PropertyName == nameof(DeleteRoleCommand.Id));
    }
}
