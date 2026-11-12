using PANiXiDA.TacticalHeroes.FileManager.Domain.Files;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.UpdateContent;

public sealed class UpdateFileContentCommandValidator : AbstractValidator<UpdateFileContentCommand>
{
    public UpdateFileContentCommandValidator()
    {
        RuleFor(command => command.Id)
            .MustBeValidDomainValue(FileId.Create);

        RuleFor(command => command.ContentType)
            .MustBeValidDomainValue(FileContentType.Create);

        RuleFor(command => command.Size)
            .MustBeValidDomainValue(FileSize.Create);

        RuleFor(command => command.Content)
            .NotNull();
    }
}
