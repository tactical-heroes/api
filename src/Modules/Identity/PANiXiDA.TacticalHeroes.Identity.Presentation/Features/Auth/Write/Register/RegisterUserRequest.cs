namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.Write.Register;

public sealed record RegisterUserRequest(
    string Email,
    string UserName,
    string Password);
