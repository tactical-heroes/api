namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.IdentityUsers.Register;

internal sealed record RegisterUserRequest(
    string Email,
    string Password);
