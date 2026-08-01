namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.Authorize;

internal sealed record AuthorizeUserResponse(
    bool IsConfirmed,
    bool IsBlocked);
