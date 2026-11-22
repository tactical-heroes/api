using PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Files;

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Files.Delete;

public sealed class DeleteFileCommandValidator : AbstractValidator<DeleteFileCommand>
{
    public DeleteFileCommandValidator()
    {
        RuleFor(command => command.Id)
            .MustBeValidDomainValue(FileId.Create);
    }
}
