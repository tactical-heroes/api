namespace PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Logout;

public sealed record LogoutCommand(Guid UserId, string AuthorizationId, string ClientId) : ICommand<Result>;
