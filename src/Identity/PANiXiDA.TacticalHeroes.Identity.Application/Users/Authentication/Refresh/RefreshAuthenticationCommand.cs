namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Authentication.Refresh;

public sealed record RefreshAuthenticationCommand(Guid UserId)
    : ICommand<Result<AuthenticatedUserReadModel>>;
