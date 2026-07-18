using System.ComponentModel.DataAnnotations;

using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.Update;

public sealed record UpdateUserRequest(
    string Email,
    string UserName,
    bool IsConfirmed,
    IReadOnlyCollection<Claim> Claims,
    [property: Required]
    [property: StringLength(50)] string Status);
