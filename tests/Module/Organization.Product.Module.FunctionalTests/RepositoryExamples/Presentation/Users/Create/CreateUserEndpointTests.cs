using System.Net;
using System.Net.Http.Json;

using Organization.Product.Module.Presentation.Features.Users.Create;
using Organization.Product.Module.Presentation.Features.Users.GetDetails;

using static Organization.Product.Module.FunctionalTests.Presentation.Users.UsersApiConstants;

namespace Organization.Product.Module.FunctionalTests.Presentation.Users.Create;

public sealed class CreateUserEndpointTests(FunctionalTestFixture fixture) : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "Post users should create user and return details when request is valid")]
    public async Task PostUsers_Should_Create_User_And_Return_Details_When_Request_Is_Valid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var birthDate = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-35);
        var request = new CreateUserRequest(
            Role: "Admin",
            Name: "John Doe",
            Email: "john@example.com",
            Phone: "+12345678901",
            BirthDate: birthDate,
            Avatar: "https://example.com/avatar.png");

        var createResponse = await Client.PostAsJsonAsync(
            UsersPath,
            request,
            JsonOptions,
            cancellationToken);

        createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        createResponse.Headers.Location.ShouldNotBeNull();

        var createdUser = await createResponse.Content.ReadFromJsonAsync<CreateUserResponse>(
            JsonOptions,
            cancellationToken);
        createdUser.ShouldNotBeNull();
        createdUser.Id.ShouldNotBe(Guid.Empty);

        var detailsResponse = await Client.GetAsync(createResponse.Headers.Location, cancellationToken);
        var details = await detailsResponse.Content.ReadFromJsonAsync<UserDetailsResponse>(
            JsonOptions,
            cancellationToken);

        detailsResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        details.ShouldNotBeNull();
        details.Id.ShouldBe(createdUser.Id);
        details.Name.ShouldBe("John Doe");
        details.Email.ShouldBe("john@example.com");
        details.Phone.ShouldBe("+12345678901");
        details.BirthDate.ShouldBe(birthDate);
        details.Avatar.ShouldBe("https://example.com/avatar.png");
    }

    [Fact(DisplayName = "Post users should return bad request when request is invalid")]
    public async Task PostUsers_Should_Return_BadRequest_When_Request_Is_Invalid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var request = new CreateUserRequest(
            Role: "",
            Name: "",
            Email: "invalid-email",
            Phone: "123",
            BirthDate: DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-17),
            Avatar: new string('a', 501));

        var response = await Client.PostAsJsonAsync(
            UsersPath,
            request,
            JsonOptions,
            cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}
