using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.CleanupVersion;

public sealed class CleanupFileVersionCommandValidator : AbstractValidator<CleanupFileVersionCommand>
{
    public CleanupFileVersionCommandValidator()
    {
        RuleFor(command => command.FileId).MustBeValueObject(FileId.Create);
        RuleFor(command => command.StorageKey).MustBeValueObject(FileStorageKey.Create);
        RuleFor(command => command.VersionId).MustBeValueObject(FileVersionId.Create);
    }
}
