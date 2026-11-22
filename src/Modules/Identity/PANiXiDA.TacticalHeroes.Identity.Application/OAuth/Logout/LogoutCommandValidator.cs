using PANiXiDA.TacticalHeroes.Identity.Domain.Users;

namespace PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Logout;

public sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(command => command.UserId).MustBeValidDomainValue(UserId.Create);
        RuleFor(command => command.AuthorizationId).NotEmpty();
        RuleFor(command => command.ClientId).NotEmpty();
    }
}
