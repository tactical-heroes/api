using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Move;

public sealed class MoveFolderCommandValidator : AbstractValidator<MoveFolderCommand>
{
    public MoveFolderCommandValidator()
    {
        RuleFor(command => command.Id)
            .MustBeValidDomainValue(FolderId.Create);

        RuleFor(command => command.ParentId)
            .MustBeValidDomainValue(value => FolderId.Create(value.GetValueOrDefault()))
            .When(command => command.ParentId is not null);
    }
}
