using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Files;
using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Users;

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Files.Update;

public sealed class UpdateFileCommandValidator : AbstractValidator<UpdateFileCommand>
{
    public UpdateFileCommandValidator()
    {
        RuleFor(command => command.Id)
            .MustBeValidDomainValue(FileId.Create);

        RuleFor(command => command.Name)
            .MustBeValidDomainValue(FileName.Create);

        RuleFor(command => command.Type)
            .MustBeValidDomainValue(FileType.Create);

        RuleFor(command => command.UserId)
            .MustBeValidDomainValue(UserId.Create);

        RuleFor(command => command.FolderId)
            .MustBeValidDomainValue(FolderId.Create);
    }
}
