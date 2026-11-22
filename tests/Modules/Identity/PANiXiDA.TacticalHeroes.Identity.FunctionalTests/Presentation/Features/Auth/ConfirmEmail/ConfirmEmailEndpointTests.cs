using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Events;
using PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Users;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.ConfirmEmail;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.Register;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.Update;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Auth.ConfirmEmail;

[Collection(FunctionalTestCollectionDefinition.Name)]
public sealed class ConfirmEmailEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    [Fact(
        DisplayName = "POST auth confirm-email should confirm the persisted user with the issued token when token is valid")]
    public async Task PostConfirmEmail_Should_ConfirmUser_When_TokenIsValid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var registerResponse = await Client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new RegisterUserRequest(
                "confirm@example.test",
                "confirm-hero",
                "StrongPassword1!"),
            JsonOptions,
            cancellationToken);
        var createdUser = await registerResponse.Content.ReadFromJsonAsync<RegisterUserResponse>(
            JsonOptions,
            cancellationToken);

        registerResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        createdUser.ShouldNotBeNull();
        var confirmation = Fixture.EventBus.Single<EmailConfirmationRequested>();

        using var response = await Client.PostAsJsonAsync(
            "/api/v1/auth/confirm-email",
            new ConfirmEmailRequest(createdUser.Id, confirmation.ConfirmationToken),
            JsonOptions,
            cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var user = await UserDatabaseTestHelper.FindAsync(Fixture, createdUser.Id, cancellationToken);
        user.ShouldNotBeNull();
        user.EmailConfirmed.ShouldBeTrue();
    }

    [Fact(DisplayName = "POST auth confirm-email should reject old proof and accept new proof when email changes")]
    public async Task PostConfirmEmail_Should_RejectOldProofAndAcceptNewProof_When_EmailChanges()
    {
        var ct = TestContext.Current.CancellationToken;
        var api = new UserApiTestClient(Fixture);
        var created = await api.CreateAsync(UserApiTestClient.CreateDefaultRequest(isConfirmed: false), ct);
        string oldToken;
        await using (var scope = Fixture.Services.CreateAsyncScope())
        {
            var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await manager.FindByIdAsync(created.Id.ToString());
            user.ShouldNotBeNull();
            oldToken = await manager.GenerateEmailConfirmationTokenAsync(user);
        }
        await api.UpdateAsync(
            created.Id, new UpdateUserRequest("changed@example.test", "hero", false, [], "Active", []), ct);

        using var response = await Client.PostAsJsonAsync(
            "/api/v1/auth/confirm-email", new ConfirmEmailRequest(created.Id, oldToken), JsonOptions, ct);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var unconfirmed = await UserDatabaseTestHelper.FindAsync(Fixture, created.Id, ct);
        unconfirmed.ShouldNotBeNull();
        unconfirmed.Email.ShouldBe("changed@example.test");
        unconfirmed.EmailConfirmed.ShouldBeFalse();
        string newToken;
        await using (var scope = Fixture.Services.CreateAsyncScope())
        {
            var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await manager.FindByIdAsync(created.Id.ToString());
            user.ShouldNotBeNull();
            newToken = await manager.GenerateEmailConfirmationTokenAsync(user);
        }
        using var confirmation = await Client.PostAsJsonAsync(
            "/api/v1/auth/confirm-email", new ConfirmEmailRequest(created.Id, newToken), JsonOptions, ct);
        confirmation.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var confirmed = await UserDatabaseTestHelper.FindAsync(Fixture, created.Id, ct);
        confirmed.ShouldNotBeNull();
        confirmed.Email.ShouldBe("changed@example.test");
        confirmed.EmailConfirmed.ShouldBeTrue();
    }
}
