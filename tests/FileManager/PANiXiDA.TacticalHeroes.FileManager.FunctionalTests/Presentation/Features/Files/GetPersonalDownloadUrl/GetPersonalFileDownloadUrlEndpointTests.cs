using System.Net.Http.Headers;

using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.Core.Application.Persistence;

using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.CreatePersonal;
using PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.GetPersonalDownloadUrl;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.FunctionalTests.Presentation.Features.Files.GetPersonalDownloadUrl;

public sealed class GetPersonalFileDownloadUrlEndpointTests(FunctionalTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    private const string Route = "/api/v1/files/personal";

    [Theory(DisplayName = "Download URL endpoint should return downloadable content when the owner uploads a file")]
    [InlineData("report 2026.txt")]
    [InlineData("отчёт 2026.txt")]
    public async Task HandleAsync_Should_ReturnDownloadableContent_When_TheOwnerUploadsAFile(string name)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var userId = Guid.NewGuid();
        byte[] bytes = [1, 2, 3, 4];
        using var uploadRequest = new HttpRequestMessage(HttpMethod.Post, Route);
        uploadRequest.Headers.Add(TestAuthenticationHandler.UserIdHeader, userId.ToString());
        using var content = new ByteArrayContent(bytes);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        uploadRequest.Content = new MultipartFormDataContent { { content, "File", name } };
        using var uploaded = await Fixture.Client.SendAsync(uploadRequest, cancellationToken);
        uploaded.StatusCode.ShouldBe(HttpStatusCode.Created);
        var file = await uploaded.Content.ReadFromJsonAsync<CreatePersonalFileResponse>(cancellationToken);
        file.ShouldNotBeNull();
        var beforeRequest = DateTimeOffset.UtcNow;

        using var response = await SendRequestAsync(file.Id, userId.ToString());

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Headers.CacheControl.ShouldNotBeNull();
        response.Headers.CacheControl.NoStore.ShouldBeTrue();
        var download = await response.Content.ReadFromJsonAsync<GetPersonalFileDownloadUrlResponse>(cancellationToken);
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
    [InlineData("avatar")]
    [InlineData("deleted")]
    public async Task HandleAsync_Should_ReturnNotFound_When_FileIsUnavailable(string fileKind)
    {
        var userId = Guid.NewGuid();
        var id = Guid.NewGuid();
        if (fileKind != "missing")
        {
            var ownerId = fileKind == "deleted" ? userId : Guid.NewGuid();
            var file = await AddFileAsync(
                fileKind == "avatar" ? FileType.Avatar : FileType.Personal,
                fileKind == "avatar" ? null : ownerId,
                "Ready",
                deleted: fileKind == "deleted");
            id = file.Id.Value;
        }

        using var response = await SendRequestAsync(id, userId.ToString());

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "Download URL endpoint should return conflict when upload is not ready")]
    public async Task HandleAsync_Should_ReturnConflict_When_UploadIsNotReady()
    {
        var userId = Guid.NewGuid();
        var file = await AddFileAsync(FileType.Personal, userId, "PendingUpload", deleted: false);

        using var response = await SendRequestAsync(file.Id.Value, userId.ToString());

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

    private async Task<HttpResponseMessage> SendRequestAsync(Guid id, string? subject)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{Route}/{id}/download-url");
        if (subject is not null)
        {
            request.Headers.Add(TestAuthenticationHandler.UserIdHeader, subject);
        }

        return await Fixture.Client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    private async Task<File> AddFileAsync(FileType type, Guid? userId, string status, bool deleted)
    {
        var file = File.Create(
            FileName.Create("file.txt").Value,
            type,
            userId is { } ownerId ? UserId.Create(ownerId).Value : null).Value;
        if (status != "PendingUpload")
        {
            file.CompleteUpload(FileContentType.Create("text/plain").Value, FileSize.Create(4).Value);
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
