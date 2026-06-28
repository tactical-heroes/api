namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.GetAuthenticated;

public sealed record AuthenticatedUserReadModel(
    Guid Id,
    string Email,
    bool ConfirmationStatus,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<AuthenticatedUserClaimReadModel> Claims)
{
    public AuthenticatedUserReadModel(
        Guid id,
        string email,
        bool confirmationStatus,
        IReadOnlyCollection<string> roles,
        IReadOnlyCollection<AuthenticatedUserClaimReadModel> claims,
        IReadOnlyCollection<AuthenticatedUserClaimReadModel> roleClaims)
        : this(
            id,
            email,
            confirmationStatus,
            roles
                .Distinct(StringComparer.Ordinal)
                .OrderBy(role => role, StringComparer.Ordinal)
                .ToArray(),
            claims
                .Concat(roleClaims)
                .Distinct()
                .OrderBy(claim => claim.Type, StringComparer.Ordinal)
                .ThenBy(claim => claim.Value, StringComparer.Ordinal)
                .ToArray())
    {
    }
}

public sealed record AuthenticatedUserClaimReadModel(
    string Type,
    string Value);
