namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Accounts.Register;

public sealed record RegisterAccountRequest(
    string Email,
    string UserName,
    string Password);
