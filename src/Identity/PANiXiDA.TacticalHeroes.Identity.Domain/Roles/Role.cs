using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Entities.RoleClaims;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Entities.RoleClaims.ValueObjects;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Domain.Roles;

public sealed class Role : AggregateRoot<RoleId>
{
    private readonly List<RoleClaim> _claims = [];

    private Role(
        RoleId id,
        RoleName name)
        : base(id)
    {
        Name = name;
    }

    public RoleName Name { get; private set; }
    public IReadOnlyCollection<RoleClaim> Claims => _claims;

    public static Role Create(
        RoleId id,
        RoleName name,
        IEnumerable<RoleClaim> claims)
    {
        var role = new Role(id: id, name: name);

        foreach (var claim in claims)
        {
            role.GrantClaim(claim);
        }

        return role;
    }

    public void GrantClaim(RoleClaim claim)
    {
        if (_claims.Any(existing => existing.Type == claim.Type && existing.Value == claim.Value))
        {
            return;
        }

        _claims.Add(claim);
    }

    public void RevokeClaim(
        ClaimType type,
        ClaimValue value)
    {
        _claims.RemoveAll(claim => claim.Type == type && claim.Value == value);
    }
}
