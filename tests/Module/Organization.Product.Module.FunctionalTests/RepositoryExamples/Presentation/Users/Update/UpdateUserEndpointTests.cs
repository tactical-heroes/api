using System.Net;
using System.Net.Http.Json;

using Organization.Product.Module.Presentation.Features.Users.Create;
using Organization.Product.Module.Presentation.Features.Users.GetDetails;
using Organization.Product.Module.Presentation.Features.Users.Update;

using static Organization.Product.Module.FunctionalTests.Presentation.Users.UsersApiConstants;

namespace Organization.Product.Module.FunctionalTests.Presentation.Users.Update;

public sealed class UpdateUserEndpointTests(FunctionalTestFixture fixture) : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "Put users should update user when request is valid")]
    public async Task PutUsers_Should_Update_User_When_Request_Is_Valid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var createResponse = await Client.PostAsJsonAsync(
            UsersPath,
            new CreateUserRequest(
                Role: "User",
                Name: "John Doe",
                Email: "old@example.com",
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

        var birthDate = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-40);
        var request = new UpdateUserRequest(
            Role: "Moderator",
            Name: "Jane Doe",
            Email: "new@example.com",
            Phone: "+19876543210",
            BirthDate: birthDate,
            Avatar: null);

        var updateResponse = await Client.PutAsJsonAsync(
            $"{UsersPath}/{createdUser.Id}",
            request,
            JsonOptions,
            cancellationToken);
        var detailsResponse = await Client.GetAsync($"{UsersPath}/{createdUser.Id}", cancellationToken);
        var details = await detailsResponse.Content.ReadFromJsonAsync<UserDetailsResponse>(
            JsonOptions,
            cancellationToken);

        updateResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        detailsResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        details.ShouldNotBeNull();
        details.Id.ShouldBe(createdUser.Id);
        details.Name.ShouldBe("Jane Doe");
        details.Email.ShouldBe("new@example.com");
        details.Phone.ShouldBe("+19876543210");
        details.BirthDate.ShouldBe(birthDate);
        details.Avatar.ShouldBeNull();
    }
}
