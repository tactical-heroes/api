using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Users;

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Files.Create;

public sealed class CreateFileCommandValidator : AbstractValidator<CreateFileCommand>
{
    public CreateFileCommandValidator()
    {
        RuleFor(command => command.Name)
            .MustBeValidDomainValue(FileName.Create);

        RuleFor(command => command.Type)
            .MustBeValidDomainValue(FileType.Create);

        RuleFor(command => command.ContentType)
            .MustBeValidDomainValue(FileContentType.Create);

        RuleFor(command => command.Size)
            .MustBeValidDomainValue(FileSize.Create);

        RuleFor(command => command.UserId)
            .MustBeValidDomainValue(UserId.Create);

        RuleFor(command => command.FolderId)
            .MustBeValidDomainValue(FolderId.Create);

        RuleFor(command => command.Content)
            .NotNull();
    }
}
