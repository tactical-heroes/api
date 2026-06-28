namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.GetAuthenticated;

public sealed record AuthenticatedUserReadModel
{
    public AuthenticatedUserReadModel(
        Guid Id,
        string Email,
        bool ConfirmationStatus,
        IReadOnlyCollection<string> Roles,
        IReadOnlyCollection<AuthenticatedUserClaimReadModel> Claims,
        IReadOnlyCollection<AuthenticatedUserClaimReadModel>? RoleClaims = null)
    {
        this.Id = Id;
        this.Email = Email;
        this.ConfirmationStatus = ConfirmationStatus;
        this.Roles = [.. Roles
            .Distinct(StringComparer.Ordinal)
            .OrderBy(role => role, StringComparer.Ordinal)];
        this.Claims = [.. Claims
            .Concat(RoleClaims ?? [])
            .Distinct()
            .OrderBy(claim => claim.Type, StringComparer.Ordinal)
            .ThenBy(claim => claim.Value, StringComparer.Ordinal)];
    }

    public Guid Id { get; }
    public string Email { get; }
    public bool ConfirmationStatus { get; }
    public IReadOnlyCollection<string> Roles { get; }
    public IReadOnlyCollection<AuthenticatedUserClaimReadModel> Claims { get; }
}

public sealed record AuthenticatedUserClaimReadModel(
    string Type,
    string Value);
