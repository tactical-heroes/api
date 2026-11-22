using PANiXiDA.TacticalHeroes.FileManager.Application.Files.ReconcileUploads;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Files.ReconcileUploads;

public sealed class ReconcileFileUploadsCommandValidatorTests
{
    [Fact(DisplayName = "Reconciliation validator should accept the command when no arguments are required")]
    public void Validate_Should_AcceptCommand_When_NoArgumentsAreRequired()
    {
        var validator = new ReconcileFileUploadsCommandValidator();

        var result = validator.Validate(new ReconcileFileUploadsCommand());

        result.IsValid.ShouldBeTrue();
    }
}
