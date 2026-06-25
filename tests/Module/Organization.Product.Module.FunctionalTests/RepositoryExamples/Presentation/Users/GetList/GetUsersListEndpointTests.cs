using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.WebUtilities;

using Organization.Product.Module.Presentation.Features.Users.Create;
using Organization.Product.Module.Presentation.Features.Users.GetList;

using static Organization.Product.Module.FunctionalTests.Presentation.Users.UsersApiConstants;

namespace Organization.Product.Module.FunctionalTests.Presentation.Users.GetList;

public sealed class GetUsersListEndpointTests(FunctionalTestFixture fixture) : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "Get users should return filtered page when query is provided")]
    public async Task GetUsers_Should_Return_Filtered_Page_When_Query_Is_Provided()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var firstCreateResponse = await Client.PostAsJsonAsync(
            UsersPath,
            new CreateUserRequest(
                Role: "Admin",
                Name: "Charlie Admin",
                Email: "charlie@example.com",
                Phone: "+12345678901",
                BirthDate: DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-30),
                Avatar: "https://example.com/avatar.png"),
            JsonOptions,
            cancellationToken);
        var secondCreateResponse = await Client.PostAsJsonAsync(
            UsersPath,
            new CreateUserRequest(
                Role: "Admin",
                Name: "Alice Admin",
                Email: "alice@example.com",
                Phone: "+12345678902",
                BirthDate: DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-30),
                Avatar: "https://example.com/avatar.png"),
            JsonOptions,
            cancellationToken);
        var thirdCreateResponse = await Client.PostAsJsonAsync(
            UsersPath,
            new CreateUserRequest(
                Role: "User",
                Name: "Bob User",
                Email: "bob@example.com",
                Phone: "+12345678903",
                BirthDate: DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-30),
                Avatar: "https://example.com/avatar.png"),
            JsonOptions,
            cancellationToken);
        var requestUri = QueryHelpers.AddQueryString(
            UsersPath,
            new Dictionary<string, string?>
            {
                ["role"] = "admin",
                ["pageNumber"] = "1",
                ["pageSize"] = "10",
                ["field"] = "Name",
                ["order"] = "Ascending"
            });

        firstCreateResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        secondCreateResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        thirdCreateResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        var response = await Client.GetAsync(requestUri, cancellationToken);
        var page = await response.Content.ReadFromJsonAsync<PaginationResult<UserListItemResponse>>(
            JsonOptions,
            cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        page.ShouldNotBeNull();
        page.TotalCount.ShouldBe(2);
        page.Items.Select(item => item.Name).ShouldBe(["Alice Admin", "Charlie Admin"]);
    }
}
