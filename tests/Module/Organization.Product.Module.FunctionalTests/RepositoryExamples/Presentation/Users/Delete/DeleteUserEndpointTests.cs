using System.Net;
using System.Net.Http.Json;

using Organization.Product.Module.Presentation.Features.Users.Create;

using static Organization.Product.Module.FunctionalTests.Presentation.Users.UsersApiConstants;

namespace Organization.Product.Module.FunctionalTests.Presentation.Users.Delete;

public sealed class DeleteUserEndpointTests(FunctionalTestFixture fixture) : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "Delete users should remove user when user exists")]
    public async Task DeleteUsers_Should_Remove_User_When_User_Exists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var createResponse = await Client.PostAsJsonAsync(
            UsersPath,
            new CreateUserRequest(
                Role: "User",
                Name: "John Doe",
                Email: "delete@example.com",
                Phone: "+12345678901",
                BirthDate: DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-30),
                Avatar: "https://example.com/avatar.png"),
            JsonOptions,
            cancellationToken);
        createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        var createdUser = await createResponse.Content.ReadFromJsonAsync<CreateUserResponse>(
            JsonOptions,
            cancellationToken);
        createdUser.ShouldNotBeNull();

        var deleteResponse = await Client.DeleteAsync($"{UsersPath}/{createdUser.Id}", cancellationToken);
        var detailsResponse = await Client.GetAsync($"{UsersPath}/{createdUser.Id}", cancellationToken);

        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        detailsResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
