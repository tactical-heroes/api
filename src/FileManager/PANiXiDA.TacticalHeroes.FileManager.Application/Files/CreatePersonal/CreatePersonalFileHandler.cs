using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Services;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.CreatePersonal;

public sealed class CreatePersonalFileHandler(FileCreationService fileCreationService, ICurrentUser currentUser)
    : ICommandHandler<CreatePersonalFileCommand, Result<Guid>>, IRequireAuthorization
{
    public Task<Result<Guid>> HandleAsync(CreatePersonalFileCommand command, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Task.FromResult(Result.Failure<Guid>(Error.Unauthorized("A user account is required.")));
        }

        return fileCreationService.CreateAsync(
            name: command.Name,
            type: FileType.Personal,
            userId: userId,
            contentType: command.ContentType,
            size: command.Size,
            content: command.Content,
            folderId: command.FolderId,
            cancellationToken: cancellationToken);
    }
}
