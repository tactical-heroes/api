using System.Net;
using System.Net.Http.Json;

using Organization.Product.Module.Presentation.Features.Users.Create;
using Organization.Product.Module.Presentation.Features.Users.GetDetails;

using static Organization.Product.Module.FunctionalTests.Presentation.Users.UsersApiConstants;

namespace Organization.Product.Module.FunctionalTests.Presentation.Users.GetDetails;

public sealed class GetUserDetailsEndpointTests(FunctionalTestFixture fixture) : FunctionalTestBase(fixture)
{
    [Fact(DisplayName = "Get user by id should return details when user exists")]
    public async Task GetUserById_Should_Return_Details_When_User_Exists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var birthDate = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-35);
        var createResponse = await Client.PostAsJsonAsync(
            UsersPath,
            new CreateUserRequest(
                Role: "Admin",
                Name: "John Details",
                Email: "details@example.com",
                Phone: "+12345678901",
                BirthDate: birthDate,
                Avatar: "https://example.com/details.png"),
            JsonOptions,
            cancellationToken);
        createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        var createdUser = await createResponse.Content.ReadFromJsonAsync<CreateUserResponse>(
            JsonOptions,
            cancellationToken);
        createdUser.ShouldNotBeNull();

        var response = await Client.GetAsync($"{UsersPath}/{createdUser.Id}", cancellationToken);
        var details = await response.Content.ReadFromJsonAsync<UserDetailsResponse>(
            JsonOptions,
            cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        details.ShouldNotBeNull();
        details.Id.ShouldBe(createdUser.Id);
        details.Name.ShouldBe("John Details");
        details.Email.ShouldBe("details@example.com");
        details.Phone.ShouldBe("+12345678901");
        details.BirthDate.ShouldBe(birthDate);
        details.Avatar.ShouldBe("https://example.com/details.png");
    }
}
