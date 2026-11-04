using PANiXiDA.Core.Application.Authentication.Abstractions;
using PANiXiDA.Core.Application.Storage.Models;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Services;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.Create;

public sealed class CreateFileHandler(FileCreationService fileCreationService)
    : ICommandHandler<CreateFileCommand, Result<Guid>>, IRequireAuthorization
{
    public Task<Result<Guid>> HandleAsync(CreateFileCommand command, CancellationToken cancellationToken)
    {
        var type = FileType.Create(command.Type);
        if (type.IsFailure)
        {
            return Task.FromResult(Result.Failure<Guid>(type.Errors));
        }

        return fileCreationService.CreateAsync(
            name: command.Name,
            type: type.Value,
            userId: command.UserId,
            metadata: new FileMetadata(command.ContentType, command.Size),
            content: command.Content,
            folderId: command.FolderId,
            cancellationToken: cancellationToken);
    }
}
