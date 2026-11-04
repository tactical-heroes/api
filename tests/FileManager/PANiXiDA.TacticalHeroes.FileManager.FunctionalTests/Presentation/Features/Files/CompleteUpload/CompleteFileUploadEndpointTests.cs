using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.Core.Application.Storage;

using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Read.DbModels;
using PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.CreateUploadUrl;
using PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.GetDownloadUrl;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.FunctionalTests.Presentation.Features.Files.CompleteUpload;

[Collection(ManualReconciliationTestCollectionDefinition.Name)]
public sealed class CompleteFileUploadEndpointTests(ManualReconciliationTestFixture fixture)
    : FunctionalTestBase(fixture)
{
    private const string Route = "/api/v1/files";

    [Theory(DisplayName = "Complete upload endpoint should allow immediate download when presigned upload succeeds")]
    [InlineData("Personal", true, false, "file-manager.personal-files.create.own")]
    [InlineData("Personal", false, true, "file-manager.personal-files.create.all")]
    [InlineData("Avatar", false, false, "file-manager.avatars.create")]
    public async Task HandleAsync_Should_AllowImmediateDownload_When_PresignedUploadSucceeds(
        string type, bool personalRoute, bool foreignOwner, string permission)
    {
        var ct = TestContext.Current.CancellationToken;
        var actorId = Guid.NewGuid();
        Guid? ownerId = type == "Personal" ? foreignOwner ? Guid.NewGuid() : actorId : null;
        var createRoute = personalRoute ? $"{Route}/personal/upload-url" : $"{Route}/upload-url";
        using var reserve = new HttpRequestMessage(HttpMethod.Post, createRoute);
        reserve.Headers.Add(TestAuthenticationHandler.UserIdHeader, actorId.ToString());
        reserve.Headers.Add(TestAuthenticationHandler.PermissionsHeader, permission);
        reserve.Content = JsonContent.Create(
            new CreateFileUploadUrlRequest("file.txt", type, "text/plain", 4, ownerId));
        using var reserved = await Fixture.Client.SendAsync(reserve, ct);
        reserved.StatusCode.ShouldBe(HttpStatusCode.Created);
        var upload = await reserved.Content.ReadFromJsonAsync<CreateFileUploadUrlResponse>(ct);
        upload.ShouldNotBeNull();
        using var premature = await SendAsync(upload.Id, actorId, permission);
        premature.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await ReadAsync(upload.Id)).Status.ShouldBe("PendingUpload");
        using var client = new HttpClient();
        using var put = new HttpRequestMessage(HttpMethod.Put, upload.Url);
        put.Content = new ByteArrayContent([1, 2, 3, 4]);
        foreach (var header in upload.RequiredHeaders)
        {
            put.Content.Headers.TryAddWithoutValidation(header.Key, header.Value).ShouldBeTrue();
        }
        using var uploaded = await client.SendAsync(put, ct);
        uploaded.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await ReadAsync(upload.Id)).Status.ShouldBe("PendingUpload");

        using var completed = await SendAsync(upload.Id, actorId, permission);

        completed.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await completed.Content.ReadAsByteArrayAsync(ct)).ShouldBeEmpty();
        var stored = await ReadAsync(upload.Id);
        stored.Status.ShouldBe("Ready");
        stored.ContentType.ShouldBe("text/plain");
        stored.Size.ShouldBe(4);
        stored.UserId.ShouldBe(ownerId);
        stored.Type.ShouldBe(type);
        stored.DeletedAt.ShouldBeNull();
        var version = await VersionAsync(upload.Id);
        using var repeated = await SendAsync(upload.Id, actorId, permission);
        repeated.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await VersionAsync(upload.Id)).ShouldBe(version);
        using var downloadRequest = new HttpRequestMessage(HttpMethod.Get, $"{Route}/{upload.Id}/download-url");
        downloadRequest.Headers.Add(TestAuthenticationHandler.UserIdHeader, actorId.ToString());
        downloadRequest.Headers.Add(
            TestAuthenticationHandler.PermissionsHeader,
            type == "Avatar" ? "file-manager.avatars.read" : foreignOwner
                ? "file-manager.personal-files.read.all" : "file-manager.personal-files.read.own");
        using var downloadResponse = await Fixture.Client.SendAsync(downloadRequest, ct);
        downloadResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var download = await downloadResponse.Content.ReadFromJsonAsync<GetFileDownloadUrlResponse>(ct);
        download.ShouldNotBeNull();
        using var downloaded = await client.GetAsync(download.Url, ct);
        downloaded.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await downloaded.Content.ReadAsByteArrayAsync(ct)).ShouldBe([1, 2, 3, 4]);
    }

    [Theory(DisplayName = "Complete upload endpoint should conceal the file when creation scope is denied")]
    [InlineData("Personal", true, "file-manager.personal-files.create.own")]
    [InlineData("Personal", false, "file-manager.personal-files.read.own")]
    [InlineData("Personal", false, "file-manager.personal-files.update.all")]
    [InlineData("Personal", false, "")]
    [InlineData("Avatar", false, "file-manager.personal-files.create.all")]
    [InlineData("Avatar", false, "file-manager.avatars.read")]
    public async Task HandleAsync_Should_ConcealFile_When_CreationScopeIsDenied(
        string type, bool foreignOwner, string permission)
    {
        var actorId = Guid.NewGuid();
        Guid? ownerId = type == "Personal" ? foreignOwner ? Guid.NewGuid() : actorId : null;
        var file = await AddFileAsync(type, ownerId);
        await UploadAsync(file, [1, 2, 3, 4]);
        var version = await VersionAsync(file.Id.Value);

        using var response = await SendAsync(file.Id.Value, actorId, permission);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await ReadAsync(file.Id.Value)).Status.ShouldBe("PendingUpload");
        (await VersionAsync(file.Id.Value)).ShouldBe(version);
    }

    [Theory(DisplayName = "Complete upload endpoint should reject lookup when identifier or file is unavailable")]
    [InlineData("empty", HttpStatusCode.BadRequest)]
    [InlineData("missing", HttpStatusCode.NotFound)]
    [InlineData("deleted", HttpStatusCode.NotFound)]
    public async Task HandleAsync_Should_RejectLookup_When_IdentifierOrFileIsUnavailable(
        string scenario, HttpStatusCode expected)
    {
        var actorId = Guid.NewGuid();
        var id = scenario == "empty" ? Guid.Empty : Guid.NewGuid();
        if (scenario == "deleted")
        {
            var file = await AddFileAsync("Personal", actorId, deleted: true);
            id = file.Id.Value;
        }

        using var response = await SendAsync(id, actorId, "file-manager.personal-files.create.own");

        response.StatusCode.ShouldBe(expected);
    }

    [Theory(DisplayName = "Complete upload endpoint should preserve pending files when stored content is unusable")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_PreservePendingFiles_When_StoredContentIsUnusable(bool emptyObject)
    {
        var actorId = Guid.NewGuid();
        var file = await AddFileAsync("Personal", actorId);
        if (emptyObject)
        {
            await UploadAsync(file, []);
        }
        var version = await VersionAsync(file.Id.Value);

        using var response = await SendAsync(file.Id.Value, actorId, "file-manager.personal-files.create.own");

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        var stored = await ReadAsync(file.Id.Value);
        stored.Status.ShouldBe("PendingUpload");
        stored.ContentType.ShouldBeNull();
        stored.Size.ShouldBeNull();
        stored.DeletedAt.ShouldBeNull();
        (await VersionAsync(file.Id.Value)).ShouldBe(version);
    }

    [Fact(DisplayName = "Complete upload endpoint should preserve deletion intent when file is being deleted")]
    public async Task HandleAsync_Should_PreserveDeletionIntent_When_FileIsBeingDeleted()
    {
        var actorId = Guid.NewGuid();
        var file = await AddFileAsync("Personal", actorId, deleting: true);
        await UploadAsync(file, [1, 2, 3, 4]);
        var version = await VersionAsync(file.Id.Value);

        using var response = await SendAsync(file.Id.Value, actorId, "file-manager.personal-files.create.own");

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        var stored = await ReadAsync(file.Id.Value);
        stored.Status.ShouldBe("Deleting");
        stored.DeletedAt.ShouldBeNull();
        (await VersionAsync(file.Id.Value)).ShouldBe(version);
    }

    [Fact(DisplayName = "Complete upload endpoint should require authentication when caller is anonymous")]
    public async Task HandleAsync_Should_RequireAuthentication_When_CallerIsAnonymous()
    {
        var actorId = Guid.NewGuid();
        var file = await AddFileAsync("Personal", actorId);

        using var response = await SendAsync(file.Id.Value, actorId: null, permission: "");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await ReadAsync(file.Id.Value)).Status.ShouldBe("PendingUpload");
    }

    private async Task<HttpResponseMessage> SendAsync(Guid id, Guid? actorId, string permission)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, $"{Route}/{id}/complete-upload");
        if (actorId is { } userId)
        {
            message.Headers.Add(TestAuthenticationHandler.UserIdHeader, userId.ToString());
        }
        message.Headers.Add(TestAuthenticationHandler.PermissionsHeader, permission);
        return await Fixture.Client.SendAsync(message, TestContext.Current.CancellationToken);
    }

    private async Task<File> AddFileAsync(string type, Guid? ownerId, bool deleted = false, bool deleting = false)
    {
        var file = File.Create(
            FileName.Create("file.txt").Value, FileType.FromName(type), UserId.Create(ownerId).Value).Value;
        if (deleting)
        {
            file.BeginDeletion();
        }
        await using var scope = Fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
        db.Add(file);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        if (deleted)
        {
            db.Remove(file);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }
        return file;
    }

    private async Task UploadAsync(File file, byte[] bytes)
    {
        await using var scope = Fixture.CreateScope();
        await using var content = new MemoryStream(bytes);
        await scope.ServiceProvider.GetRequiredService<IFileStorage>()
            .UploadAsync(file.StorageKey.Value, content, "text/plain", TestContext.Current.CancellationToken);
    }

    private async Task<FileReadDbModel> ReadAsync(Guid id)
    {
        await using var scope = Fixture.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<FileManagerReadDbContext>()
            .Set<FileReadDbModel>()
            .IgnoreQueryFilters()
            .SingleAsync(file => file.Id == id, TestContext.Current.CancellationToken);
    }

    private async Task<uint> VersionAsync(Guid id)
    {
        await using var scope = Fixture.CreateScope();
        var fileId = FileId.Create(id).Value;
        return await scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>()
            .Set<File>()
            .Where(file => file.Id == fileId)
            .Select(file => EF.Property<uint>(file, "xmin"))
            .SingleAsync(TestContext.Current.CancellationToken);
    }
}
