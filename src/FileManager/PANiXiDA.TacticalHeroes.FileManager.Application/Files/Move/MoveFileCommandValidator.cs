using PANiXiDA.TacticalHeroes.FileManager.Domain.Files;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.Move;

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
