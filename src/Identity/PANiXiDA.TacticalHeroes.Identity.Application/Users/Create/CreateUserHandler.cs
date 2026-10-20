using PANiXiDA.Core.Application.Authentication.Abstractions;
using PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserClaims;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserClaims.ValueObjects;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Create;

public sealed class CreateUserHandler(IUsersRepository usersRepository)
    : ICommandHandler<CreateUserCommand, Result<Guid>>, IRequireAuthorization
{
    public static IReadOnlyCollection<string> AllPermissions { get; } =
    [
        Permissions.IdentityUsers.Create.Code
    ];

    public Task<Result<Guid>> HandleAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        var emailResult = Email.Create(value: command.Email);
        var userNameResult = UserName.Create(value: command.UserName);
        var statusResult = UserStatus.Create(value: command.Status);
        var roleIdsResult = RoleId.Create(command.RoleIds);
        var validationResult = Result.Combine(emailResult, userNameResult, statusResult, roleIdsResult);

        if (validationResult.IsFailure)
        {
            return Task.FromResult(Result.Failure<Guid>(errors: validationResult.Errors));
        }

        var claims = new List<UserClaim>();

        foreach (var claim in command.Claims)
        {
            var typeResult = ClaimType.Create(value: claim.Type);
            var valueResult = ClaimValue.Create(value: claim.Value);
            var claimResult = Result.Combine(typeResult, valueResult);

            if (claimResult.IsFailure)
            {
                return Task.FromResult(Result.Failure<Guid>(errors: claimResult.Errors));
            }

            claims.Add(UserClaim.Create(type: typeResult.Value, value: valueResult.Value));
        }

        var user = User.Create(
            id: UserId.New(),
            email: emailResult.Value,
            userName: userNameResult.Value,
            status: statusResult.Value,
            confirmationStatus: UserConfirmationStatus.From(isConfirmed: command.IsConfirmed),
            roleIds: roleIdsResult.Value,
            claims: claims);

        return usersRepository.AddAsync(
            user: user,
            password: command.Password,
            cancellationToken: cancellationToken);
    }
}
