using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Roles.Create;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Roles.GetDetails;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Roles.GetList;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Roles.Update;

using static PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Roles.RolesApiConstants;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Roles;

internal sealed class RoleApiTestClient(FunctionalTestFixture fixture)
{
    internal async Task<CreateRoleResponse> CreateAsync(
        CancellationToken cancellationToken,
        CreateRoleRequest? request = null)
    {
        request ??= new CreateRoleRequest("admin", []);
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, RolesPath)
        {
            Content = JsonContent.Create(request, options: TestJsonSerializerOptions.Web)
        };
        using var response = await OAuthServiceAccessTokenTestHelper.SendAsync(
            fixture,
            httpRequest,
            cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Created, responseBody);
        response.Headers.Location.ShouldNotBeNull();

        return await response.Content.ReadFromJsonAsync<CreateRoleResponse>(
                TestJsonSerializerOptions.Web,
                cancellationToken)
            ?? throw new InvalidOperationException("Created role was not returned.");
    }

    internal async Task<GetRoleDetailsResponse> GetDetailsAsync(
        Guid roleId,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{RolesPath}/{roleId}");
        using var response = await OAuthServiceAccessTokenTestHelper.SendAsync(
            fixture,
            request,
            cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, responseBody);

        return await response.Content.ReadFromJsonAsync<GetRoleDetailsResponse>(
                TestJsonSerializerOptions.Web,
                cancellationToken)
            ?? throw new InvalidOperationException("Role details were not returned.");
    }

    internal async Task<PaginationResult<RoleListItemResponse>> GetListAsync(
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{RolesPath}?pageNumber=1&pageSize=20");
        using var response = await OAuthServiceAccessTokenTestHelper.SendAsync(
            fixture,
            request,
            cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, responseBody);

        return await response.Content.ReadFromJsonAsync<PaginationResult<RoleListItemResponse>>(
                TestJsonSerializerOptions.Web,
                cancellationToken)
            ?? throw new InvalidOperationException("Role page was not returned.");
    }

    internal Task UpdateAsync(
        Guid roleId,
        UpdateRoleRequest request,
        CancellationToken cancellationToken)
    {
        return SendWithoutContentAsync(
            HttpMethod.Put,
            $"{RolesPath}/{roleId}",
            JsonContent.Create(request, options: TestJsonSerializerOptions.Web),
            cancellationToken);
    }

    internal Task DeleteAsync(Guid roleId, CancellationToken cancellationToken)
    {
        return SendWithoutContentAsync(
            HttpMethod.Delete,
            $"{RolesPath}/{roleId}",
            null,
            cancellationToken);
    }

    private async Task SendWithoutContentAsync(
        HttpMethod method,
        string path,
        HttpContent? content,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, path)
        {
            Content = content
        };
        using var response = await OAuthServiceAccessTokenTestHelper.SendAsync(
            fixture,
            request,
            cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent, responseBody);
    }
}
