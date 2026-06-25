namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.Register;

internal sealed record RegisterUserRequest(
    string Email,
    string Password);
