using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Roles.Create;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Roles;

public sealed class RoleNameEndpointTests(FunctionalTestFixture fixture) : FunctionalTestBase(fixture)
{
    [Theory(DisplayName = "Role requests should reject empty names when name is empty")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SaveRole_Should_ReturnBadRequest_When_NameIsEmpty(bool update)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new RoleApiTestClient(Fixture);
        var roles = await client.GetListAsync(cancellationToken);
        var roleId = roles.Items.Single(role => role.Name == "Player").Id;
        var path = update ? $"/api/v1/roles/{roleId}" : "/api/v1/roles";
        using var request = new HttpRequestMessage(update ? HttpMethod.Put : HttpMethod.Post, path)
        {
            Content = JsonContent.Create(new CreateRoleRequest(" ", false, []), options: JsonOptions)
        };

        using var response = await OAuthServiceAccessTokenTestHelper.SendAsync(Fixture, request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var details = await client.GetDetailsAsync(roleId, cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest, body);
        details.Name.ShouldBe("Player");
    }
}
