using System.Reflection;

using PANiXiDA.Core.Application.Authentication.Abstractions;
using PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;
using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Create;
using PANiXiDA.TacticalHeroes.Identity.Application.Users.Create;

namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Application;

public sealed class PermissionAuthorizationTests
{
    [Fact(DisplayName = "Handlers should require known permissions when managing resources")]
    public void Handlers_Should_RequireKnownPermissions_When_ManagingResources()
    {
        var handlers = new[] { typeof(CreateFactionHandler).Assembly, typeof(CreateUserHandler).Assembly }
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.Name.EndsWith("Handler", StringComparison.Ordinal)
                && type.Namespace?.Split('.').Any(part =>
                    part is "Factions" or "Heroes" or "Units" or "Roles" or "Users" or "Claims") == true)
            .ToArray();

        Assert.NotEmpty(handlers);
        foreach (var handler in handlers)
        {
            Assert.True(typeof(IRequireAuthorization).IsAssignableFrom(handler), handler.FullName);
            var permissions = new[] { "AllPermissions", "AnyPermissions" }
                .Select(name => handler.GetProperty(name, BindingFlags.Public | BindingFlags.Static))
                .Where(property => property is not null)
                .SelectMany(property => (IReadOnlyCollection<string>)property!.GetValue(null)!)
                .ToArray();
            Assert.NotEmpty(permissions);
            Assert.All(permissions, permission => Assert.Contains(Permissions.All, item => item.Code == permission));
        }
    }
}
