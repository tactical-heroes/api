using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Files;
using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Folders;

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Files.Move;

public sealed class MoveFileCommandValidator : AbstractValidator<MoveFileCommand>
{
    public MoveFileCommandValidator()
    {
        RuleFor(command => command.Id)
            .MustBeValidDomainValue(FileId.Create);

        RuleFor(command => command.FolderId)
            .MustBeValidDomainValue(FolderId.Create);
    }
}
