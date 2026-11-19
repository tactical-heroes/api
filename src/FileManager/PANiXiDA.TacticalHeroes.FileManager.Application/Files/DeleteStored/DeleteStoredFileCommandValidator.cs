using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.DeleteStored;

public sealed class DeleteStoredFileCommandValidator : AbstractValidator<DeleteStoredFileCommand>
{
    public DeleteStoredFileCommandValidator()
    {
        RuleFor(command => command.StorageKey).MustBeValueObject(FileStorageKey.Create);
        RuleFor(command => command.RecheckUntil).NotEmpty();
    }
}
