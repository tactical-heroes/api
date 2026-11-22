using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Folders;

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Folders.Delete;

public sealed class DeleteFolderCommandValidator : AbstractValidator<DeleteFolderCommand>
{
    public DeleteFolderCommandValidator()
    {
        RuleFor(command => command.Id)
            .MustBeValidDomainValue(FolderId.Create);
    }
}
