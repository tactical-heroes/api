using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.Create;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.GetDetails;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.GetList;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.GetStatuses;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.Update;

using static PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Users.UsersApiConstants;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Users;

internal sealed class UserApiTestClient(FunctionalTestFixture fixture)
{
    internal async Task<CreateUserResponse> CreateAsync(
        CancellationToken cancellationToken,
        CreateUserRequest? request = null)
    {
        request ??= CreateDefaultRequest();
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, UsersPath)
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
        var user = await response.Content.ReadFromJsonAsync<CreateUserResponse>(
            TestJsonSerializerOptions.Web,
            cancellationToken);

        return user ?? throw new InvalidOperationException("Created user was not returned.");
    }

    internal async Task<GetUserDetailsResponse> GetDetailsAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{UsersPath}/{userId}");
        using var response = await OAuthServiceAccessTokenTestHelper.SendAsync(
            fixture,
            request,
            cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, responseBody);

        return await response.Content.ReadFromJsonAsync<GetUserDetailsResponse>(
                TestJsonSerializerOptions.Web,
                cancellationToken)
            ?? throw new InvalidOperationException("User details were not returned.");
    }

    internal async Task<PaginationResult<UserListItemResponse>> GetListAsync(
        CancellationToken cancellationToken,
        string? email = null,
        int pageNumber = 1,
        int pageSize = 20)
    {
        var path = $"{UsersPath}?pageNumber={pageNumber}&pageSize={pageSize}";

        if (!string.IsNullOrWhiteSpace(email))
        {
            path = $"{path}&email={Uri.EscapeDataString(email)}";
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        using var response = await OAuthServiceAccessTokenTestHelper.SendAsync(
            fixture,
            request,
            cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, responseBody);

        return await response.Content.ReadFromJsonAsync<PaginationResult<UserListItemResponse>>(
                TestJsonSerializerOptions.Web,
                cancellationToken)
            ?? throw new InvalidOperationException("User page was not returned.");
    }

    internal async Task<IReadOnlyCollection<UserStatusResponse>> GetStatusesAsync(
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{UsersPath}/statuses");
        using var response = await OAuthServiceAccessTokenTestHelper.SendAsync(
            fixture,
            request,
            cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, responseBody);

        return await response.Content.ReadFromJsonAsync<IReadOnlyCollection<UserStatusResponse>>(
                TestJsonSerializerOptions.Web,
                cancellationToken)
            ?? throw new InvalidOperationException("User statuses were not returned.");
    }

    internal Task UpdateAsync(
        Guid userId,
        UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        return SendWithoutContentAsync(
            HttpMethod.Put,
            $"{UsersPath}/{userId}",
            JsonContent.Create(request, options: TestJsonSerializerOptions.Web),
            cancellationToken);
    }

    internal Task DeleteAsync(Guid userId, CancellationToken cancellationToken)
    {
        return SendWithoutContentAsync(
            HttpMethod.Delete,
            $"{UsersPath}/{userId}",
            null,
            cancellationToken);
    }

    internal Task BlockAsync(Guid userId, CancellationToken cancellationToken)
    {
        return SendWithoutContentAsync(
            HttpMethod.Post,
            $"{UsersPath}/{userId}/block",
            null,
            cancellationToken);
    }

    internal Task UnblockAsync(Guid userId, CancellationToken cancellationToken)
    {
        return SendWithoutContentAsync(
            HttpMethod.Post,
            $"{UsersPath}/{userId}/unblock",
            null,
            cancellationToken);
    }

    internal static CreateUserRequest CreateDefaultRequest(
        string email = "hero@example.com",
        string userName = "hero",
        bool isConfirmed = true)
    {
        return new CreateUserRequest(
            email,
            userName,
            "StrongPassword1!",
            isConfirmed,
            [],
            "Active");
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
