using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.Core.Application.Persistence;

using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.GetPersonalDetails;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.FunctionalTests.Presentation.Features.Files.GetPersonalDetails;

public sealed class GetPersonalFileDetailsEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    private const string Route = "/api/v1/files/personal";

    [Fact(DisplayName = "GET personal file should return pending metadata when user owns file")]
    public async Task HandleAsync_Should_ReturnPendingMetadata_When_UserOwnsFile()
    {
        var userId = Guid.NewGuid();
        var file = await AddFileAsync(FileType.Personal, userId);

        using var response = await SendRequestAsync(file.Id.Value, userId.ToString());

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var details = await response.Content.ReadFromJsonAsync<GetPersonalFileDetailsResponse>(
            TestContext.Current.CancellationToken);
        details.ShouldNotBeNull();
        details.Id.ShouldBe(file.Id.Value);
        details.Name.ShouldBe(file.Name.Value);
        details.FolderId.ShouldBeNull();
        details.Status.ShouldBe("PendingUpload");
        details.ContentType.ShouldBeNull();
        details.Size.ShouldBeNull();
    }

    [Theory(DisplayName = "GET personal file should return not found when file is unavailable")]
    [InlineData("missing")]
    [InlineData("foreign")]
    [InlineData("avatar")]
    public async Task HandleAsync_Should_ReturnNotFound_When_FileIsUnavailable(string fileKind)
    {
        var userId = Guid.NewGuid();
        var id = Guid.NewGuid();
        if (fileKind != "missing")
        {
            var file = await AddFileAsync(
                fileKind == "avatar" ? FileType.Avatar : FileType.Personal,
                fileKind == "avatar" ? null : Guid.NewGuid());
            id = file.Id.Value;
        }

        using var response = await SendRequestAsync(id, userId.ToString());

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Theory(DisplayName = "GET personal file should reject authentication when subject is unavailable")]
    [InlineData(null)]
    [InlineData("invalid-subject")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task HandleAsync_Should_ReturnUnauthorized_When_SubjectIsUnavailable(string? subject)
    {
        using var response = await SendRequestAsync(Guid.NewGuid(), subject);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "GET personal file should reject the identifier when id is empty")]
    public async Task HandleAsync_Should_ReturnBadRequest_When_IdIsEmpty()
    {
        using var response = await SendRequestAsync(Guid.Empty, Guid.NewGuid().ToString());

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    private async Task<HttpResponseMessage> SendRequestAsync(Guid id, string? subject)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{Route}/{id}");
        if (subject is not null)
        {
            request.Headers.Add(TestAuthenticationHandler.UserIdHeader, subject);
        }

        return await Fixture.Client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    private async Task<File> AddFileAsync(FileType type, Guid? userId)
    {
        var file = File.Create(
            FileName.Create("file.txt").Value,
            type,
            userId is { } ownerId ? UserId.Create(ownerId).Value : null).Value;
        await using var scope = Fixture.CreateScope();
        scope.ServiceProvider.GetRequiredService<IFilesRepository>().Add(file);
        await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext))
            .SaveChangesAsync(TestContext.Current.CancellationToken);

        return file;
    }
}
