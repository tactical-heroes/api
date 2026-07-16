using System.ComponentModel.DataAnnotations;

using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Accounts.Management.Create;

public sealed record CreateAccountRequest(
    string Email,
    string UserName,
    string Password,
    bool IsConfirmed,
    IReadOnlyCollection<Claim> Claims,
    [property: Required]
    [property: StringLength(50)] string Status);
