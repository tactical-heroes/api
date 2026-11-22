using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Folders.ValueObjects;
using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Users;

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Folders.Update;

public sealed class UpdateFolderCommandValidator : AbstractValidator<UpdateFolderCommand>
{
    public UpdateFolderCommandValidator()
    {
        RuleFor(command => command.Id)
            .MustBeValidDomainValue(FolderId.Create);

        RuleFor(command => command.Name)
            .MustBeValidDomainValue(FolderName.Create);

        RuleFor(command => command.AllowedFileType)
            .MustBeValidDomainValue(FileType.Create);

        RuleFor(command => command.UserId)
            .MustBeValidDomainValue(UserId.Create);

        RuleFor(command => command.ParentId)
            .MustBeValidDomainValue(FolderId.Create);
    }
}
