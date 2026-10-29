using System.Text.Json;

using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.Core.Application.Persistence;

using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.GetDetails;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.FunctionalTests.Presentation.Features.Files.GetDetails;

public sealed class GetFileDetailsEndpointTests(FunctionalTestFixture fixture) : FunctionalTestBase(fixture)
{
    private const string Route = "/api/v1/files";

    [Theory(DisplayName = "GET file should return pending metadata when reading is permitted")]
    [InlineData("Personal", "file-manager.personal-files.read.own")]
    [InlineData("Avatar", "file-manager.avatars.read")]
    public async Task HandleAsync_Should_ReturnPendingMetadata_When_ReadingIsPermitted(string type, string permission)
    {
        var userId = Guid.NewGuid();
        var file = await AddFileAsync(type, type == "Personal" ? userId : null);

        using var response = await SendAsync(file.Id.Value, userId.ToString(), permission);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var details = await response.Content
            .ReadFromJsonAsync<GetFileDetailsResponse>(TestContext.Current.CancellationToken);
        details.ShouldNotBeNull();
        details.Id.ShouldBe(file.Id.Value);
        details.Name.ShouldBe(file.Name.Value);
        details.Type.ShouldBe(type);
        details.FolderId.ShouldBeNull();
        details.Status.ShouldBe("PendingUpload");
        details.ContentType.ShouldBeNull();
        details.Size.ShouldBeNull();
        using var json = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        json.RootElement.EnumerateObject().Select(property => property.Name)
            .ShouldBe(["id", "name", "type", "folderId", "status", "contentType", "size"], ignoreOrder: true);
    }

    [Fact(DisplayName = "GET file should allow another owner when read all is granted")]
    public async Task HandleAsync_Should_AllowAnotherOwner_When_ReadAllIsGranted()
    {
        var file = await AddFileAsync("Personal", Guid.NewGuid(), completed: true);

        using var response = await SendAsync(
            file.Id.Value,
            Guid.NewGuid().ToString(),
            "file-manager.personal-files.read.all");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var details = await response.Content
            .ReadFromJsonAsync<GetFileDetailsResponse>(TestContext.Current.CancellationToken);
        details.ShouldNotBeNull();
        details.Status.ShouldBe("Ready");
        details.ContentType.ShouldBe("text/plain");
        details.Size.ShouldBe(4);
    }

    [Theory(DisplayName = "GET file should conceal metadata when file is missing deleted or forbidden")]
    [InlineData("missing", "file-manager.personal-files.read.all")]
    [InlineData("deleted", "file-manager.personal-files.read.all")]
    [InlineData("foreign", "file-manager.personal-files.read.own")]
    [InlineData("personal", "file-manager.avatars.read")]
    [InlineData("avatar", "file-manager.personal-files.read.all")]
    [InlineData("personal", "")]
    public async Task HandleAsync_Should_ConcealMetadata_When_FileIsMissingDeletedOrForbidden(
        string kind,
        string permission)
    {
        var userId = Guid.NewGuid();
        var file = await AddFileAsync(
            kind == "avatar" ? "Avatar" : "Personal",
            kind == "avatar" ? null : kind == "foreign" ? Guid.NewGuid() : userId,
            deleted: kind == "deleted");

        using var response = await SendAsync(
            kind == "missing" ? Guid.NewGuid() : file.Id.Value,
            userId.ToString(),
            permission);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "GET file should use stored ownership when query parameters spoof access metadata")]
    public async Task HandleAsync_Should_UseStoredOwnership_When_QueryParametersSpoofAccessMetadata()
    {
        var userId = Guid.NewGuid();
        var file = await AddFileAsync("Personal", Guid.NewGuid());
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{Route}/{file.Id.Value}?userId={userId}&type=Avatar");
        request.Headers.Add(TestAuthenticationHandler.UserIdHeader, userId.ToString());
        request.Headers.Add(
            TestAuthenticationHandler.PermissionsHeader,
            "file-manager.avatars.read,file-manager.personal-files.read.own");

        using var response = await Fixture.Client.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "GET file should return unauthorized when caller is unauthenticated")]
    public async Task HandleAsync_Should_ReturnUnauthorized_When_CallerIsUnauthenticated()
    {
        using var response = await SendAsync(Guid.NewGuid(), subject: null, "file-manager.personal-files.read.all");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "GET file should reject the identifier when id is empty")]
    public async Task HandleAsync_Should_ReturnBadRequest_When_IdIsEmpty()
    {
        using var response = await SendAsync(
            Guid.Empty,
            Guid.NewGuid().ToString(),
            "file-manager.personal-files.read.all");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "GET file should replace the personal route when general details are requested")]
    public async Task HandleAsync_Should_ReplacePersonalRoute_When_GeneralDetailsAreRequested()
    {
        var userId = Guid.NewGuid();
        var file = await AddFileAsync("Personal", userId);
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{Route}/personal/{file.Id.Value}");
        request.Headers.Add(TestAuthenticationHandler.UserIdHeader, userId.ToString());
        request.Headers.Add(TestAuthenticationHandler.PermissionsHeader, "file-manager.personal-files.read.own");

        using var response = await Fixture.Client.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    private async Task<HttpResponseMessage> SendAsync(Guid id, string? subject, string permission)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{Route}/{id}");
        if (subject is not null)
        {
            request.Headers.Add(TestAuthenticationHandler.UserIdHeader, subject);
        }
        request.Headers.Add(TestAuthenticationHandler.PermissionsHeader, permission);
        return await Fixture.Client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    private async Task<File> AddFileAsync(string type, Guid? userId, bool completed = false, bool deleted = false)
    {
        var file = File.Create(
            FileName.Create("file.txt").Value,
            FileType.FromName(type),
            userId is { } owner ? UserId.Create(owner).Value : null).Value;
        if (completed) file.CompleteUpload(FileContentType.Create("text/plain").Value, FileSize.Create(4).Value);
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IFilesRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext));
        repository.Add(file);
        await unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
        if (deleted)
        {
            repository.Delete(file);
            await unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
        }
        return file;
    }
}
