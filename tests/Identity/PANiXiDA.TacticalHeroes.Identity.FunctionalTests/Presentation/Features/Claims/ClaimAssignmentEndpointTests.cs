using System.Text.Json;
using System.Text.Json.Nodes;

using PANiXiDA.TacticalHeroes.Common.Authentication;
using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth;
using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Roles;
using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Users;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Roles.Create;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Roles.Update;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.Update;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Claims;

public sealed class ClaimAssignmentEndpointTests(FunctionalTestFixture fixture) : FunctionalTestBase(fixture)
{
    [Theory(DisplayName = "User and role requests should reject unknown managed claims when assigning claims")]
    [InlineData(false, false, "permission")]
    [InlineData(false, true, "permission")]
    [InlineData(true, false, "permission")]
    [InlineData(true, true, "permission")]
    [InlineData(false, false, "section")]
    [InlineData(false, true, "section")]
    [InlineData(true, false, "section")]
    [InlineData(true, true, "section")]
    public async Task AssignClaims_Should_ReturnBadRequest_When_ManagedClaimIsUnknown(
        bool role,
        bool update,
        string type)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var path = role ? "/api/v1/roles" : "/api/v1/users";
        var payload = CreatePayload(role, new Claim(type, "unknown"));
        if (update)
        {
            var id = role
                ? (await new RoleApiTestClient(Fixture).CreateAsync(cancellationToken)).Id
                : (await new UserApiTestClient(Fixture).CreateAsync(cancellationToken)).Id;
            path = $"{path}/{id}";
            payload.Remove("password");
        }

        using var request = new HttpRequestMessage(update ? HttpMethod.Put : HttpMethod.Post, path)
        {
            Content = JsonContent.Create(payload, options: JsonOptions)
        };
        using var response = await OAuthServiceAccessTokenTestHelper.SendAsync(Fixture, request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest, body);
        using var problem = JsonDocument.Parse(body);
        problem.RootElement.GetProperty("errors").GetProperty("claims[0].Value")
            .EnumerateArray().Select(error => error.GetString())
            .ShouldBe(["Claim value is not present in its catalog."]);
        if (update)
        {
            using var detailsRequest = new HttpRequestMessage(HttpMethod.Get, path);
            using var detailsResponse = await OAuthServiceAccessTokenTestHelper.SendAsync(
                Fixture, detailsRequest, cancellationToken);
            detailsResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
            var detailsBody = await detailsResponse.Content.ReadAsStringAsync(cancellationToken);
            using var details = JsonDocument.Parse(detailsBody);
            details.RootElement.GetProperty("claims").GetArrayLength().ShouldBe(0);
        }
    }

    [Theory(DisplayName = "Role requests should persist a section without permissions when assigning section access")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task AssignRoleClaims_Should_PersistSectionOnly_When_SectionIsKnown(bool update)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new RoleApiTestClient(Fixture);
        Claim[] claims = [new(CustomClaimTypes.Section, "identity")];
        var role = await client.CreateAsync(cancellationToken, new CreateRoleRequest("admin", update ? [] : claims));

        if (update)
        {
            await client.UpdateAsync(role.Id, new UpdateRoleRequest("admin", claims), cancellationToken);
        }
        var details = await client.GetDetailsAsync(role.Id, cancellationToken);

        details.Claims.ShouldBe(claims);
    }

    [Theory(DisplayName = "User requests should persist a section without permissions when assigning section access")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task AssignUserClaims_Should_PersistSectionOnly_When_SectionIsKnown(bool update)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var client = new UserApiTestClient(Fixture);
        Claim[] claims = [new(CustomClaimTypes.Section, "compendium")];
        var request = UserApiTestClient.CreateDefaultRequest() with { Claims = update ? [] : claims };
        var user = await client.CreateAsync(cancellationToken, request);

        if (update)
        {
            await client.UpdateAsync(
                user.Id,
                new UpdateUserRequest(
                    Email: request.Email,
                    UserName: request.UserName,
                    IsConfirmed: request.IsConfirmed,
                    Claims: claims,
                    Status: request.Status),
                cancellationToken);
        }
        var details = await client.GetDetailsAsync(user.Id, cancellationToken);

        details.Claims.ShouldBe(claims);
    }

    private static JsonObject CreatePayload(bool role, Claim claim)
    {
        return role
            ? JsonSerializer.SerializeToNode(new CreateRoleRequest("admin", [claim]), JsonOptions)!.AsObject()
            : JsonSerializer.SerializeToNode(
                UserApiTestClient.CreateDefaultRequest() with { Claims = [claim] }, JsonOptions)!.AsObject();
    }
}
