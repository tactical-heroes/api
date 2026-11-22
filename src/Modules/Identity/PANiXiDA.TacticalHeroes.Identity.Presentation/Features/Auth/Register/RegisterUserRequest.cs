namespace PANiXiDA.TacticalHeroes.Modules.Identity.Presentation.Features.Auth.Register;

public sealed record RegisterUserRequest(
    string Email,
    string UserName,
    string Password);
