using System.ComponentModel.DataAnnotations;

using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.Create;

public sealed record CreateUserRequest(
    string Email,
    string UserName,
    string Password,
    bool IsConfirmed,
    IReadOnlyCollection<Claim> Claims,
    [property: Required]
    [property: StringLength(50)] string Status);
