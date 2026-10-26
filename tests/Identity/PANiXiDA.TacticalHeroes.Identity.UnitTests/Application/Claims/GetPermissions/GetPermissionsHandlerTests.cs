using PANiXiDA.TacticalHeroes.Identity.Application.Claims.GetPermissions;

using PermissionCatalog = PANiXiDA.TacticalHeroes.Common.Authentication.Permissions.Permissions;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Claims.GetPermissions;

public sealed class GetPermissionsHandlerTests
{
    [Fact(DisplayName = "HandleAsync should return permission codes and display metadata when query is handled")]
    public async Task HandleAsync_Should_ReturnPermissionCodesAndDisplayMetadata_When_QueryIsHandled()
    {
        var handler = new GetPermissionsHandler();

        var result = await handler.HandleAsync(new GetPermissionsQuery(), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Select(permission => permission.Code)
            .ShouldBe(PermissionCatalog.All.Select(permission => permission.Code));
        foreach (var permission in PermissionCatalog.All)
        {
            var readModel = result.Value.Single(item => item.Code == permission.Code);
            readModel.DisplayName.ShouldBe(permission.DisplayName);
            readModel.ModuleCode.ShouldBe(permission.ModuleCode);
            readModel.ResourceCode.ShouldBe(permission.ResourceCode);
            readModel.ResourceDisplayName.ShouldBe(permission.ResourceDisplayName);
        }
    }
}
