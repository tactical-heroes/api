using PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Folders.ReconcileDeletions;

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.UnitTests.Application.Folders.ReconcileDeletions;

public sealed class ReconcileFolderDeletionsCommandValidatorTests
{
    [Fact(DisplayName = "Folder reconciliation validator should accept the command when no arguments are required")]
    public void Validate_Should_AcceptCommand_When_NoArgumentsAreRequired()
    {
        var validator = new ReconcileFolderDeletionsCommandValidator();

        var result = validator.Validate(new ReconcileFolderDeletionsCommand());

        result.IsValid.ShouldBeTrue();
    }
}
