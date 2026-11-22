using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Services;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Services;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.Update;

public sealed class UpdateFileHandler(
    IFilesRepository filesRepository,
    FolderAccessService folderAccessService,
    FileAccessPolicy fileAccessPolicy,
    IUnitOfWork unitOfWork,
    FileRelocationService relocationService)
    : ICommandHandler<UpdateFileCommand, Result>, IRequireAuthorization
{
    public async Task<Result> HandleAsync(UpdateFileCommand command, CancellationToken cancellationToken)
    {
        var id = FileId.Create(command.Id);
        var name = FileName.Create(command.Name);
        var type = FileType.Create(command.Type);
        var ownerId = UserId.Create(command.UserId);
        var folderId = FolderId.Create(command.FolderId);
        var validation = Result.Combine(id, name, type, ownerId, folderId);
        if (validation.IsFailure)
        {
            return Result.Failure(validation.Errors);
        }

        var file = await filesRepository.GetByIdAsync(id.Value, cancellationToken);
        if (file is null || !fileAccessPolicy.CanUpdate(file))
        {
            return Result.Failure(Error.NotFound("File was not found."));
        }

        if (!fileAccessPolicy.CanUpdate(type.Value, ownerId.Value))
        {
            return Result.Failure(Error.Forbidden("Access to the requested file type or owner is denied."));
        }

        if ((file.Type != type.Value || file.UserId != ownerId.Value)
            && file.Size?.Value > FileRelocationPolicy.MaximumCopySize)
        {
            return Result.Failure(Error.Validation("Files larger than 5 GiB cannot change type or owner."));
        }

        var result = await UpdateAsync(
            file: file,
            name: name.Value,
            type: type.Value,
            ownerId: ownerId.Value,
            folderId: folderId.Value,
            cancellationToken: cancellationToken);
        if (result.IsFailure || file.Status != FileStatus.Relocating)
        {
            return result;
        }

        return await relocationService.RelocateAsync(file, cancellationToken);
    }

    private async Task<Result> UpdateAsync(
        File file,
        FileName name,
        FileType type,
        UserId? ownerId,
        FolderId? folderId,
        CancellationToken cancellationToken)
    {
        var result = Result.Success();
        await unitOfWork.ExecuteInTransactionAsync(
            action: async token =>
            {
                Folder? folder = null;
                if (folderId is { } targetId)
                {
                    var destination = await folderAccessService.GetForUpdateAsync(targetId, token);
                    if (destination.IsFailure)
                    {
                        result = Result.Failure(destination.Errors);
                        return;
                    }

                    folder = destination.Value;
                }

                result = file.Update(name: name, type: type, userId: ownerId, folder: folder);
                if (result.IsFailure)
                {
                    return;
                }

                filesRepository.Update(file);
                await unitOfWork.SaveChangesAsync(token);
            },
            cancellationToken: cancellationToken);

        return result;
    }
}
