using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.CreatePersonalUploadUrl;

public sealed class CreatePersonalFileUploadUrlCommandValidator : AbstractValidator<CreatePersonalFileUploadUrlCommand>
{
    public CreatePersonalFileUploadUrlCommandValidator()
    {
        RuleFor(command => command.Name)
            .MustBeValidDomainValue(FileName.Create);

        RuleFor(command => command.ContentType)
            .MustBeValidDomainValue(FileContentType.Create);

        RuleFor(command => command.Size)
            .MustBeValidDomainValue(FileSize.Create);

        RuleFor(command => command.FolderId)
            .MustBeValidDomainValue(FolderId.Create);
    }
}
