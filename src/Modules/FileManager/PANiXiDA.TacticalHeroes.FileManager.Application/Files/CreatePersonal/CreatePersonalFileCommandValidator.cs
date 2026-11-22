using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.CreatePersonal;

public sealed class CreatePersonalFileCommandValidator : AbstractValidator<CreatePersonalFileCommand>
{
    public CreatePersonalFileCommandValidator()
    {
        RuleFor(command => command.Name)
            .MustBeValidDomainValue(FileName.Create);

        RuleFor(command => command.ContentType)
            .MustBeValidDomainValue(FileContentType.Create);

        RuleFor(command => command.Size)
            .MustBeValidDomainValue(FileSize.Create);

        RuleFor(command => command.FolderId)
            .MustBeValidDomainValue(FolderId.Create);

        RuleFor(command => command.Content)
            .NotNull();
    }
}
