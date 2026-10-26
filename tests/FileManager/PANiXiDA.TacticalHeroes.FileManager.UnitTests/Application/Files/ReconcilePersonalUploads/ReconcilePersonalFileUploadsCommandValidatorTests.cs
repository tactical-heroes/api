using PANiXiDA.TacticalHeroes.FileManager.Application.Files.ReconcilePersonalUploads;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Files.ReconcilePersonalUploads;

public sealed class ReconcilePersonalFileUploadsCommandValidatorTests
{
    [Fact(DisplayName = "Reconciliation validator should accept the command when no arguments are required")]
    public void Validate_Should_AcceptCommand_When_NoArgumentsAreRequired()
    {
        var validator = new ReconcilePersonalFileUploadsCommandValidator();

        var result = validator.Validate(new ReconcilePersonalFileUploadsCommand());

        result.IsValid.ShouldBeTrue();
    }
}
