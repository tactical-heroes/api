using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Files;
using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Files.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Files.UpdateContent;

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
