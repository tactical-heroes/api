using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Files;
using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Files.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Files.Rename;

public sealed class RenameFileCommandValidator : AbstractValidator<RenameFileCommand>
{
    public RenameFileCommandValidator()
    {
        RuleFor(command => command.Id)
            .MustBeValidDomainValue(FileId.Create);

        RuleFor(command => command.Name)
            .MustBeValidDomainValue(FileName.Create);
    }
}
