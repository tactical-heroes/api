using PANiXiDA.Core.Application.Authentication.Abstractions;
using PANiXiDA.Core.Application.Storage.Models;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Common.Models;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Services;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.CreateUploadUrl;

public sealed class CreateFileUploadUrlHandler(FileCreationService fileCreationService)
    : ICommandHandler<CreateFileUploadUrlCommand, Result<FileUploadUrl>>, IRequireAuthorization, IRequireTransaction
{
    public Task<Result<FileUploadUrl>> HandleAsync(
        CreateFileUploadUrlCommand command,
        CancellationToken cancellationToken)
    {
        var type = FileType.Create(command.Type);
        if (type.IsFailure)
        {
            return Task.FromResult(Result.Failure<FileUploadUrl>(type.Errors));
        }

        return fileCreationService.CreateUploadUrlAsync(
            name: command.Name,
            type: type.Value,
            userId: command.UserId,
            metadata: new FileMetadata(command.ContentType, command.Size),
            folderId: command.FolderId,
            cancellationToken: cancellationToken);
    }
}
