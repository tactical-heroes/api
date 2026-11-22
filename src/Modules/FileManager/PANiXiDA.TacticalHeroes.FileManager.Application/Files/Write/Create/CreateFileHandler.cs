using System.Collections.ObjectModel;

using PANiXiDA.Core.Application.Authentication.Abstractions;
using PANiXiDA.Core.Application.Storage.Models;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Services;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.Write.Create;

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
            metadata: new FileMetadata(
                ContentType: command.ContentType,
                Size: command.Size,
                Metadata: ReadOnlyDictionary<string, string>.Empty),
            content: command.Content,
            folderId: command.FolderId,
            cancellationToken: cancellationToken);
    }
}
