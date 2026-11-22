using PANiXiDA.TacticalHeroes.FileManager.Domain.Files;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.Write.CompleteUpload;

public sealed class CompleteFileUploadCommandValidator : AbstractValidator<CompleteFileUploadCommand>
{
    public CompleteFileUploadCommandValidator()
    {
        RuleFor(command => command.Id)
            .MustBeValidDomainValue(FileId.Create);
    }
}
