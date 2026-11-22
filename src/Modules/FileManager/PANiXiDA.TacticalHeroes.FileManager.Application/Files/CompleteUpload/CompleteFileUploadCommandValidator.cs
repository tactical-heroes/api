using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Files;

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Files.CompleteUpload;

public sealed class CompleteFileUploadCommandValidator : AbstractValidator<CompleteFileUploadCommand>
{
    public CompleteFileUploadCommandValidator()
    {
        RuleFor(command => command.Id)
            .MustBeValidDomainValue(FileId.Create);
    }
}
