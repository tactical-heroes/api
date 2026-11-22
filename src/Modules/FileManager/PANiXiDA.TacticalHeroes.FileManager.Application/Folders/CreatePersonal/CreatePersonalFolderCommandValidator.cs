using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Folders.CreatePersonal;

public sealed class CreatePersonalFolderCommandValidator : AbstractValidator<CreatePersonalFolderCommand>
{
    public CreatePersonalFolderCommandValidator()
    {
        RuleFor(command => command.Name)
            .MustBeValidDomainValue(FolderName.Create);

        RuleFor(command => command.ParentId)
            .MustBeValidDomainValue(value => FolderId.Create(value.GetValueOrDefault()))
            .When(command => command.ParentId is not null);
    }
}
