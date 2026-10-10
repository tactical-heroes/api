using PANiXiDA.Core.Application.Storage;

using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.CreatePersonal;

public sealed class CreatePersonalFileHandler(
    IFilesRepository filesRepository,
    IFoldersRepository foldersRepository,
    IFileStorage fileStorage,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreatePersonalFileCommand, Result<Guid>>
{
    public async Task<Result<Guid>> HandleAsync(
        CreatePersonalFileCommand command,
        CancellationToken cancellationToken)
    {
        var nameResult = FileName.Create(command.Name);
        var contentTypeResult = FileContentType.Create(command.ContentType);
        var sizeResult = FileSize.Create(command.Size);
        var userIdResult = UserId.Create(command.UserId);
        var validationResult = Result.Combine(
            nameResult,
            contentTypeResult,
            sizeResult,
            userIdResult);

        if (validationResult.IsFailure)
        {
            return Result.Failure<Guid>(validationResult.Errors);
        }

        if (command.Content is not { CanRead: true })
        {
            return Result.Failure<Guid>(Error.Validation("File content must be readable."));
        }

        var file = File.Create(
            name: nameResult.Value,
            type: FileType.Personal,
            userId: userIdResult.Value).Value;

        if (command.FolderId is { } folderId)
        {
            var folderIdResult = FolderId.Create(folderId);
            if (folderIdResult.IsFailure)
            {
                return Result.Failure<Guid>(folderIdResult.Errors);
            }

            var folder = await foldersRepository.GetByIdAsync(
                id: folderIdResult.Value,
                cancellationToken: cancellationToken);

            if (folder is null || folder.UserId != file.UserId)
            {
                return Result.Failure<Guid>(Error.NotFound("Folder was not found."));
            }

            var moveResult = file.MoveTo(folder);
            if (moveResult.IsFailure)
            {
                return Result.Failure<Guid>(moveResult.Errors);
            }
        }

        await filesRepository.AddAsync(
            aggregateRoot: file,
            cancellationToken: cancellationToken);
        await unitOfWork.CommitTransactionAsync(cancellationToken);

        await fileStorage.UploadAsync(
            key: file.StorageKey.Value,
            content: command.Content,
            contentType: contentTypeResult.Value.Value,
            cancellationToken: cancellationToken);

        var completeResult = file.CompleteUpload(
            contentType: contentTypeResult.Value,
            size: sizeResult.Value);
        if (completeResult.IsFailure)
        {
            return Result.Failure<Guid>(completeResult.Errors);
        }

        await filesRepository.UpdateAsync(
            aggregateRoot: file,
            cancellationToken: cancellationToken);

        return Result.Success(file.Id.Value);
    }
}
