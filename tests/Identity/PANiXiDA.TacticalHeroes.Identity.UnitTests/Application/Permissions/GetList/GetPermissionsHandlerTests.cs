using PANiXiDA.TacticalHeroes.Common.Authentication.Sections;
using PANiXiDA.TacticalHeroes.Identity.Application.Permissions.GetList;

using PermissionCatalog = PANiXiDA.TacticalHeroes.Common.Authentication.Permissions.Permissions;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Application.Permissions.GetList;

public sealed class GetPermissionsHandlerTests
{
    [Fact(DisplayName = "HandleAsync should return permission codes and display metadata when query is handled")]
    public async Task HandleAsync_Should_ReturnPermissionCodesAndDisplayMetadata_When_QueryIsHandled()
    {
        var handler = new GetPermissionsHandler();

        var result = await handler.HandleAsync(new GetPermissionsQuery(), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(PermissionCatalog.All.Count);
        foreach (var permission in PermissionCatalog.All)
        {
            var readModel = result.Value.Single(item => item.Code == permission.Code);
            readModel.DisplayName.ShouldBe(permission.DisplayName);
            readModel.SectionCode.ShouldBe(permission.SectionCode);
            readModel.SectionDisplayName.ShouldBe(Sections.All
                .Single(section => section.Code == permission.SectionCode)
                .DisplayName);
            readModel.ResourceCode.ShouldBe(permission.ResourceCode);
            readModel.ResourceDisplayName.ShouldBe(permission.ResourceDisplayName);
        }
    }
}
