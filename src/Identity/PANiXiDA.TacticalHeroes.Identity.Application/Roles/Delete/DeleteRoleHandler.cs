using PANiXiDA.Core.Application.Authentication.Abstractions;
using PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Roles.Delete;

public sealed class DeleteRoleHandler(IRolesRepository rolesRepository, ICurrentUser currentUser)
    : ICommandHandler<DeleteRoleCommand, Result>, IRequireAuthorization, IRequireTransaction
{
    public static IReadOnlyCollection<string> AllPermissions { get; } = [Permissions.Identity.Roles.Delete.Code];

    public Task<Result> HandleAsync(
        DeleteRoleCommand command,
        CancellationToken cancellationToken)
    {
        return rolesRepository.DeleteAsync(
            id: command.Id,
            cancellationToken: cancellationToken,
            currentAccessToken: currentUser.AccessToken);
    }
}
