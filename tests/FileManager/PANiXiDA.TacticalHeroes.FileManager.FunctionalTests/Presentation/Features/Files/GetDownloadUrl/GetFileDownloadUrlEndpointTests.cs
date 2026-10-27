using System.Net.Http.Headers;

using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.Core.Application.Persistence;
using PANiXiDA.Core.Application.Storage;

using PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;

using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.CreatePersonal;
using PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.GetDownloadUrl;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.FunctionalTests.Presentation.Features.Files.GetDownloadUrl;

public sealed class GetFileDownloadUrlEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    private const string Route = "/api/v1/files";

    [Theory(DisplayName = "Download URL endpoint should return downloadable content when access is granted")]
    [InlineData("Personal", "report 2026.txt")]
    [InlineData("Personal", "отчёт 2026.txt")]
    [InlineData("Avatar", "avatar 2026.png")]
    [InlineData("Avatar", "аватар 2026.png")]
    public async Task HandleAsync_Should_ReturnDownloadableContent_When_AccessIsGranted(string typeName, string name)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var userId = Guid.NewGuid();
        byte[] bytes = [1, 2, 3, 4];
        var type = FileType.FromName(typeName);
        Guid fileId;
        if (type == FileType.Personal)
        {
            using var uploadRequest = new HttpRequestMessage(HttpMethod.Post, Route + "/personal");
            uploadRequest.Headers.Add(TestAuthenticationHandler.UserIdHeader, userId.ToString());
            using var content = new ByteArrayContent(bytes);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            uploadRequest.Content = new MultipartFormDataContent { { content, "File", name } };
            using var uploaded = await Fixture.Client.SendAsync(uploadRequest, cancellationToken);
            uploaded.StatusCode.ShouldBe(HttpStatusCode.Created);
            var file = await uploaded.Content.ReadFromJsonAsync<CreatePersonalFileResponse>(cancellationToken);
            file.ShouldNotBeNull();
            fileId = file.Id;
        }
        else
        {
            var file = await AddFileAsync(type, null, "Ready", deleted: false, name: name);
            await using var scope = Fixture.CreateScope();
            await using var content = new MemoryStream(bytes);
            await scope.ServiceProvider.GetRequiredService<IFileStorage>().UploadAsync(
                file.StorageKey.Value, content, "application/octet-stream", cancellationToken);
            fileId = file.Id.Value;
        }

        var beforeRequest = DateTimeOffset.UtcNow;

        using var response = await SendRequestAsync(fileId, userId.ToString(), ReadPermission(type));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Headers.CacheControl.ShouldNotBeNull();
        response.Headers.CacheControl.NoStore.ShouldBeTrue();
        var download = await response.Content.ReadFromJsonAsync<GetFileDownloadUrlResponse>(cancellationToken);
        download.ShouldNotBeNull();
        download.ExpiresAt.ShouldBeGreaterThan(beforeRequest.AddMinutes(14));
        download.ExpiresAt.ShouldBeLessThan(DateTimeOffset.UtcNow.AddMinutes(16));
        var uri = new Uri(download.Url);
        uri.GetLeftPart(UriPartial.Authority).ShouldBe(Fixture.Storage.ServiceUrl);
        uri.Query.ShouldContain("X-Amz-Signature=");
        using var client = new HttpClient();
        using var downloaded = await client.GetAsync(uri, cancellationToken);
        downloaded.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await downloaded.Content.ReadAsByteArrayAsync(cancellationToken)).ShouldBe(bytes);
        downloaded.Content.Headers.ContentType!.MediaType.ShouldBe("application/octet-stream");
        downloaded.Content.Headers.ContentDisposition.ShouldNotBeNull();
        downloaded.Content.Headers.ContentDisposition.DispositionType.ShouldBe("attachment");
        downloaded.Content.Headers.ContentDisposition.FileNameStar.ShouldBe(name);
    }

    [Theory(DisplayName = "Download URL endpoint should return not found when file is unavailable")]
    [InlineData("missing")]
    [InlineData("foreign")]
    [InlineData("deleted-avatar")]
    [InlineData("deleted")]
    public async Task HandleAsync_Should_ReturnNotFound_When_FileIsUnavailable(string fileKind)
    {
        var userId = Guid.NewGuid();
        var id = Guid.NewGuid();
        var type = fileKind == "deleted-avatar" ? FileType.Avatar : FileType.Personal;
        if (fileKind != "missing")
        {
            var ownerId = fileKind == "foreign" ? Guid.NewGuid() : userId;
            var file = await AddFileAsync(
                type,
                type == FileType.Avatar ? null : ownerId,
                "Ready",
                deleted: fileKind is "deleted" or "deleted-avatar");
            id = file.Id.Value;
        }

        using var response = await SendRequestAsync(id, userId.ToString(), ReadPermission(type));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Theory(DisplayName = "Download URL endpoint should hide the file when the matching permission is missing")]
    [InlineData("Personal", "")]
    [InlineData("Personal", "file-manager.avatars.read")]
    [InlineData("Avatar", "")]
    [InlineData("Avatar", "file-manager.personal-files.read")]
    public async Task HandleAsync_Should_ReturnNotFound_When_TheMatchingPermissionIsMissing(
        string typeName,
        string permission)
    {
        var userId = Guid.NewGuid();
        var type = FileType.FromName(typeName);
        var file = await AddFileAsync(
            type, type == FileType.Personal ? userId : null, "Ready", deleted: false);

        using var response = await SendRequestAsync(file.Id.Value, userId.ToString(), permission);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "Download URL endpoint should return conflict when upload is not ready")]
    public async Task HandleAsync_Should_ReturnConflict_When_UploadIsNotReady()
    {
        var userId = Guid.NewGuid();
        var file = await AddFileAsync(FileType.Personal, userId, "PendingUpload", deleted: false);

        using var response = await SendRequestAsync(
            file.Id.Value, userId.ToString(), Permissions.FileManager.PersonalFiles.Read.Code);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Theory(DisplayName = "Download URL endpoint should reject authentication when subject is unavailable")]
    [InlineData(null)]
    [InlineData("invalid-subject")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task HandleAsync_Should_ReturnUnauthorized_When_SubjectIsUnavailable(string? subject)
    {
        using var response = await SendRequestAsync(Guid.NewGuid(), subject);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "Download URL endpoint should reject the identifier when id is empty")]
    public async Task HandleAsync_Should_ReturnBadRequest_When_IdIsEmpty()
    {
        using var response = await SendRequestAsync(Guid.Empty, Guid.NewGuid().ToString());

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    private static string ReadPermission(FileType type)
    {
        return type == FileType.Personal
            ? Permissions.FileManager.PersonalFiles.Read.Code
            : Permissions.FileManager.Avatars.Read.Code;
    }

    private async Task<HttpResponseMessage> SendRequestAsync(Guid id, string? subject, string permission = "")
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{Route}/{id}/download-url");
        if (subject is not null)
        {
            request.Headers.Add(TestAuthenticationHandler.UserIdHeader, subject);
        }

        request.Headers.Add(TestAuthenticationHandler.PermissionsHeader, permission);
        return await Fixture.Client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    private async Task<File> AddFileAsync(
        FileType type,
        Guid? userId,
        string status,
        bool deleted,
        string name = "file.txt")
    {
        var file = File.Create(
            FileName.Create(name).Value,
            type,
            userId is { } ownerId ? UserId.Create(ownerId).Value : null).Value;
        if (status != "PendingUpload")
        {
            file.CompleteUpload(FileContentType.Create("application/octet-stream").Value, FileSize.Create(4).Value);
        }

        if (deleted)
        {
            file.BeginDeletion();
        }

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
