using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.Register;

public sealed class RegisterUserHandler(IUserCredentialsService userCredentialsService)
    : ICommandHandler<RegisterUserCommand, Result<Guid>>
{
    public Task<Result<Guid>> HandleAsync(
        RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        var emailResult = Email.Create(value: command.Email);
        var userNameResult = UserName.Create(value: command.UserName);
        var validationResult = Result.Combine(emailResult, userNameResult);

        if (validationResult.IsFailure)
        {
            return Task.FromResult(Result.Failure<Guid>(errors: validationResult.Errors));
        }

        return userCredentialsService.RegisterAsync(
            user: User.Register(email: emailResult.Value),
            userName: userNameResult.Value,
            password: command.Password,
            cancellationToken: cancellationToken);
    }
}
