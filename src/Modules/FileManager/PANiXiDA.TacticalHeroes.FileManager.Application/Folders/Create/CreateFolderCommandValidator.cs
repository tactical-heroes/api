using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Folders.ValueObjects;
using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Users;

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Folders.Create;

public sealed class CreateFolderCommandValidator : AbstractValidator<CreateFolderCommand>
{
    public CreateFolderCommandValidator()
    {
        RuleFor(command => command.Name)
            .MustBeValidDomainValue(FolderName.Create);

        RuleFor(command => command.AllowedFileType)
            .MustBeValidDomainValue(FileType.Create);

        RuleFor(command => command.UserId)
            .MustBeValidDomainValue(UserId.Create);

        RuleFor(command => command.ParentId)
            .MustBeValidDomainValue(value => FolderId.Create(value.GetValueOrDefault()))
            .When(command => command.ParentId is not null);
    }
}
