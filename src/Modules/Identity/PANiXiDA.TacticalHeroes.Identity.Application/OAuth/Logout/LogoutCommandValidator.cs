using PANiXiDA.TacticalHeroes.Modules.Identity.Domain.Users;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Application.OAuth.Logout;

public sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(command => command.UserId).MustBeValidDomainValue(UserId.Create);
        RuleFor(command => command.AuthorizationId).NotEmpty();
        RuleFor(command => command.ClientId).NotEmpty();
    }
}
