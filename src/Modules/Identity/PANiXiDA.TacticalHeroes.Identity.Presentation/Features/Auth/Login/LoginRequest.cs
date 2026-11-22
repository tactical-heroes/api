namespace PANiXiDA.TacticalHeroes.Modules.Identity.Presentation.Features.Auth.Login;

public sealed record LoginRequest(
    string Email,
    string Password,
    string ReturnUrl);
