using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;
using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Specifications;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Domain.Roles;

public sealed class RoleSpecificationsTests
{
    [Fact(DisplayName = "Roles by ids specification should match valid role ids")]
    public void RolesByIdsSpecification_Should_MatchValidRoleIds()
    {
        var adminRole = Role.Create("admin").Value;
        var playerRole = Role.Create("player").Value;
        var specification = new RolesByIdsSpecification(
            [
                adminRole.Id.Value,
                Guid.Empty
            ]);

        specification.IsSatisfiedBy(adminRole).ShouldBeTrue();
        specification.IsSatisfiedBy(playerRole).ShouldBeFalse();
    }
}
