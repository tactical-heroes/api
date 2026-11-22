using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Files;
using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Files.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Files.CleanupVersion;

public sealed class CleanupFileVersionCommandValidator : AbstractValidator<CleanupFileVersionCommand>
{
    public CleanupFileVersionCommandValidator()
    {
        RuleFor(command => command.FileId)
            .MustBeValidDomainValue(FileId.Create);

        RuleFor(command => command.StorageKey)
            .MustBeValidDomainValue(FileStorageKey.Create);
        RuleFor(command => command.VersionId)
            .MustBeValidDomainValue(FileVersionId.Create);
    }
}
