using PANiXiDA.Core.Application.Authentication.Abstractions;
using PANiXiDA.Core.Application.Storage.Models;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Common.Models;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Services;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.CreatePersonalUploadUrl;

public sealed class CreatePersonalFileUploadUrlHandler(
    FileCreationService fileCreationService,
    ICurrentUser currentUser)
    :
        ICommandHandler<CreatePersonalFileUploadUrlCommand, Result<FileUploadUrl>>,
        IRequireAuthorization,
        IRequireTransaction
{
    public Task<Result<FileUploadUrl>> HandleAsync(
        CreatePersonalFileUploadUrlCommand command,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId || userId == Guid.Empty)
        {
            return Task.FromResult(Result.Failure<FileUploadUrl>(Error.Unauthorized("A user account is required.")));
        }

        return fileCreationService.CreateUploadUrlAsync(
            name: command.Name,
            type: FileType.Personal,
            userId: userId,
            metadata: new FileMetadata(command.ContentType, command.Size),
            folderId: command.FolderId,
            cancellationToken: cancellationToken);
    }
}
