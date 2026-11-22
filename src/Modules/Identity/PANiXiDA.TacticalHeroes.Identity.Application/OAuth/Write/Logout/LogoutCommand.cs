namespace PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Write.Logout;

public sealed record LogoutCommand(Guid UserId, string AuthorizationId, string ClientId) : ICommand<Result>;
