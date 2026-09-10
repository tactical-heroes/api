using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserClaims;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Entities.UserClaims.ValueObjects;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Update;

public sealed class UpdateUserHandler(IUsersRepository usersRepository)
    : ICommandHandler<UpdateUserCommand, Result>
{
    public Task<Result> HandleAsync(
        UpdateUserCommand command,
        CancellationToken cancellationToken)
    {
        var idResult = UserId.Create(value: command.Id);
        var emailResult = Email.Create(value: command.Email);
        var userNameResult = UserName.Create(value: command.UserName);
        var statusResult = UserStatus.Create(value: command.Status);
        var validationResult = Result.Combine(idResult, emailResult, userNameResult, statusResult);

        if (validationResult.IsFailure)
        {
            return Task.FromResult(Result.Failure(errors: validationResult.Errors));
        }

        var claims = new List<UserClaim>();

        foreach (var claim in command.Claims)
        {
            var typeResult = ClaimType.Create(value: claim.Type);
            var valueResult = ClaimValue.Create(value: claim.Value);
            var claimResult = Result.Combine(typeResult, valueResult);

            if (claimResult.IsFailure)
            {
                return Task.FromResult(Result.Failure(errors: claimResult.Errors));
            }

            claims.Add(UserClaim.Create(type: typeResult.Value, value: valueResult.Value));
        }

        var user = User.Create(
            id: idResult.Value,
            email: emailResult.Value,
            userName: userNameResult.Value,
            status: statusResult.Value,
            confirmationStatus: UserConfirmationStatus.From(isConfirmed: command.IsConfirmed),
            roleIds: [],
            claims: claims);

        return usersRepository.UpdateAsync(
            user: user,
            cancellationToken: cancellationToken);
    }
}
