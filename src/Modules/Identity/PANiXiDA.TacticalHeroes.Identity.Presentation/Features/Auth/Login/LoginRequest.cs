namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.Login;

public sealed record LoginRequest(
    string Email,
    string Password,
    string ReturnUrl);
