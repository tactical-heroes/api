namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.GetAuthenticated;

public sealed record AuthenticatedUserReadModel
{
    public static AuthenticatedUserReadModel Create(
        Guid id,
        string email,
        bool isConfirmed,
        IReadOnlyCollection<string> roles,
        IReadOnlyCollection<AuthenticatedUserClaimReadModel> claims)
    {
        return new AuthenticatedUserReadModel(
            id,
            email,
            isConfirmed,
            roles,
            claims);
    }

    public AuthenticatedUserReadModel(
        Guid id,
        string email,
        bool confirmationStatus,
        IReadOnlyCollection<AuthenticatedUserAssignedRoleReadModel> roles,
        IReadOnlyCollection<AuthenticatedUserClaimReadModel> claims)
        : this(
            id,
            email,
            confirmationStatus,
            roles.Select(role => role.Name).ToArray(),
            [.. claims, .. roles.SelectMany(role => role.Claims)])
    {
    }

    private AuthenticatedUserReadModel(
        Guid id,
        string email,
        bool isConfirmed,
        IReadOnlyCollection<string> roles,
        IReadOnlyCollection<AuthenticatedUserClaimReadModel> claims)
    {
        Id = id;
        Email = email;
        IsConfirmed = isConfirmed;
        Roles = [.. roles
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];
        Claims = [.. claims
            .Distinct()
            .OrderBy(claim => claim.Type, StringComparer.Ordinal)
            .ThenBy(claim => claim.Value, StringComparer.Ordinal)];
    }

    public Guid Id { get; }
    public string Email { get; }
    public bool IsConfirmed { get; }
    public IReadOnlyCollection<string> Roles { get; }
    public IReadOnlyCollection<AuthenticatedUserClaimReadModel> Claims { get; }
}
