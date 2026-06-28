namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.GetAuthenticated;

public sealed record AuthenticatedUserAssignedRoleReadModel
{
    public AuthenticatedUserAssignedRoleReadModel(
        AuthenticatedUserRoleReadModel? role)
    {
        Name = role?.Name ?? string.Empty;
        Claims = role?.Claims ?? [];
    }

    public string Name { get; }
    public IReadOnlyCollection<AuthenticatedUserClaimReadModel> Claims { get; }
}
