using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.Core.Application.Storage;

using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Read.DbModels;
using PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.GetDownloadUrl;
using PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.Rename;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.FunctionalTests.Presentation.Features.Files.Rename;

[Collection(ManualReconciliationTestCollectionDefinition.Name)]
public sealed class RenameFileEndpointTests(ManualReconciliationTestFixture fixture) : FunctionalTestBase(fixture)
{
    private const string Route = "/api/v1/files";

    [Theory(DisplayName = "PATCH file name should respect update scopes when type and ownership vary")]
    [InlineData("Personal", true, "file-manager.personal-files.update.own", true)]
    [InlineData("Personal", false, "file-manager.personal-files.update.own", false)]
    [InlineData("Personal", false, "file-manager.personal-files.update.all", true)]
    [InlineData("Personal", true, "file-manager.personal-files.read.all", false)]
    [InlineData("Personal", true, "file-manager.personal-files.create.own", false)]
    [InlineData("Personal", true, "file-manager.personal-files.delete.all", false)]
    [InlineData("Personal", true, "file-manager.avatars.update", false)]
    [InlineData("Personal", true, "", false)]
    [InlineData("Avatar", false, "file-manager.avatars.update", true)]
    [InlineData("Avatar", false, "file-manager.personal-files.update.all", false)]
    [InlineData("Avatar", false, "file-manager.avatars.read", false)]
    public async Task HandleAsync_Should_RespectUpdateScopes_When_TypeAndOwnershipVary(
        string type, bool own, string permission, bool allowed)
    {
        var userId = Guid.NewGuid();
        var file = CreateFile(type, own ? userId : Guid.NewGuid());
        await SaveAsync(file);

        using var response = await SendAsync(
            file.Id.Value, new RenameFileRequest(" Heroes "), userId.ToString(), permission);

        response.StatusCode.ShouldBe(allowed ? HttpStatusCode.NoContent : HttpStatusCode.NotFound);
        var stored = await ReadAsync(file.Id.Value);
        stored.Name.ShouldBe(allowed ? "Heroes" : "file.txt");
        stored.Type.ShouldBe(type);
        stored.UserId.ShouldBe(file.UserId?.Value);
        stored.FolderId.ShouldBeNull();
        stored.StorageKey.ShouldBe(file.StorageKey.Value);
        stored.Status.ShouldBe(FileStatus.PendingUpload.Name);
        stored.ContentType.ShouldBeNull();
        stored.Size.ShouldBeNull();
        stored.DeletedAt.ShouldBeNull();
        if (allowed)
        {
            (await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken)).ShouldBeEmpty();
        }
    }

    [Theory(DisplayName = "PATCH file name should preserve the name when request is invalid")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(".")]
    [InlineData("..")]
    [InlineData("folder/child")]
    [InlineData("folder\\child")]
    [InlineData("folder\n")]
    public async Task HandleAsync_Should_PreserveName_When_RequestIsInvalid(string? name)
    {
        var userId = Guid.NewGuid();
        var file = CreateFile("Personal", userId);
        await SaveAsync(file);

        using var response = await SendAsync(
            file.Id.Value, new { Name = name }, userId.ToString(), "file-manager.personal-files.update.own");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ReadAsync(file.Id.Value)).Name.ShouldBe("file.txt");
    }

    [Fact(DisplayName = "PATCH file name should require the name when body omits it")]
    public async Task HandleAsync_Should_RequireName_When_BodyOmitsIt()
    {
        var userId = Guid.NewGuid();
        var file = CreateFile("Personal", userId);
        await SaveAsync(file);

        using var response = await SendAsync(
            file.Id.Value, new { }, userId.ToString(), "file-manager.personal-files.update.own");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ReadAsync(file.Id.Value)).Name.ShouldBe("file.txt");
    }

    [Theory(DisplayName = "PATCH file name should reject the request when target or account is unavailable")]
    [InlineData("missing", 404)]
    [InlineData("deleted", 404)]
    [InlineData("empty", 400)]
    [InlineData("anonymous", 401)]
    public async Task HandleAsync_Should_RejectRequest_When_TargetOrAccountIsUnavailable(string kind, int code)
    {
        var userId = Guid.NewGuid();
        var file = CreateFile("Personal", userId);
        await SaveAsync(file, deleted: kind == "deleted");

        using var response = await SendAsync(
            kind == "missing" ? Guid.NewGuid() : kind == "empty" ? Guid.Empty : file.Id.Value,
            new RenameFileRequest("Heroes"),
            kind == "anonymous" ? null : userId.ToString(),
            "file-manager.personal-files.update.own");

        ((int)response.StatusCode).ShouldBe(code);
        (await ReadAsync(file.Id.Value)).Name.ShouldBe("file.txt");
    }

    [Fact(DisplayName = "PATCH file name should accept retries when same name is supplied again")]
    public async Task HandleAsync_Should_AcceptRetries_When_SameNameIsSuppliedAgain()
    {
        var userId = Guid.NewGuid();
        var file = CreateFile("Personal", userId);
        await SaveAsync(file);
        var request = new RenameFileRequest("Heroes");
        using var first = await SendAsync(
            file.Id.Value, request, userId.ToString(), "file-manager.personal-files.update.own");

        using var second = await SendAsync(
            file.Id.Value, request, userId.ToString(), "file-manager.personal-files.update.own");

        first.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        second.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await ReadAsync(file.Id.Value)).Name.ShouldBe("Heroes");
    }

    [Theory(DisplayName = "PATCH file name should preserve content and placement when file is ready")]
    [InlineData("Personal")]
    [InlineData("Avatar")]
    public async Task HandleAsync_Should_PreserveContentAndPlacement_When_FileIsReady(string type)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var userId = Guid.NewGuid();
        var file = CreateFile(type, userId);
        var folder = Folder.Create(FolderName.Create("Documents").Value, file.Type, file.UserId).Value;
        file.MoveTo(folder).IsSuccess.ShouldBeTrue();
        byte[] bytes = [1, 2, 3, 4];
        file.CompleteUpload(FileContentType.Create("text/plain").Value, FileSize.Create(bytes.Length).Value);
        await using (var scope = Fixture.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
            db.AddRange(folder, file);
            await db.SaveChangesAsync(cancellationToken);
            await using var content = new MemoryStream(bytes);
            await scope.ServiceProvider.GetRequiredService<IFileStorage>().UploadAsync(
                file.StorageKey.Value, content, "text/plain", cancellationToken);
        }
        var permission = type == "Personal" ? "file-manager.personal-files.update.own" : "file-manager.avatars.update";

        using var response = await SendAsync(
            file.Id.Value, new RenameFileRequest(" отчёт 2026.txt "), userId.ToString(), permission);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var stored = await ReadAsync(file.Id.Value);
        stored.Name.ShouldBe("отчёт 2026.txt");
        stored.StorageKey.ShouldBe(file.StorageKey.Value);
        stored.Type.ShouldBe(type);
        stored.UserId.ShouldBe(file.UserId?.Value);
        stored.FolderId.ShouldBe(folder.Id.Value);
        stored.Status.ShouldBe(FileStatus.Ready.Name);
        stored.ContentType.ShouldBe("text/plain");
        stored.Size.ShouldBe(bytes.Length);
        stored.DeletedAt.ShouldBeNull();
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{Route}/{file.Id.Value}/download-url");
        request.Headers.Add(TestAuthenticationHandler.UserIdHeader, userId.ToString());
        request.Headers.Add(
            TestAuthenticationHandler.PermissionsHeader,
            type == "Personal" ? "file-manager.personal-files.read.own" : "file-manager.avatars.read");
        using var downloadResponse = await Fixture.Client.SendAsync(request, cancellationToken);
        downloadResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var download = await downloadResponse.Content.ReadFromJsonAsync<GetFileDownloadUrlResponse>(cancellationToken);
        download.ShouldNotBeNull();
        using var client = new HttpClient();
        using var downloaded = await client.GetAsync(download.Url, cancellationToken);
        downloaded.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await downloaded.Content.ReadAsByteArrayAsync(cancellationToken)).ShouldBe(bytes);
        downloaded.Content.Headers.ContentType!.MediaType.ShouldBe("text/plain");
        downloaded.Content.Headers.ContentDisposition!.FileNameStar.ShouldBe("отчёт 2026.txt");
    }

    [Theory(DisplayName = "PATCH file name should reject rename when deletion has started")]
    [InlineData("Personal", "file-manager.personal-files.update.own")]
    [InlineData("Avatar", "file-manager.avatars.update")]
    public async Task HandleAsync_Should_RejectRename_When_DeletionHasStarted(string type, string permission)
    {
        var userId = Guid.NewGuid();
        var file = CreateFile(type, userId);
        file.BeginDeletion();
        await SaveAsync(file);

        using var response = await SendAsync(
            file.Id.Value, new RenameFileRequest("new.txt"), userId.ToString(), permission);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        var stored = await ReadAsync(file.Id.Value);
        stored.Name.ShouldBe("file.txt");
        stored.Status.ShouldBe(FileStatus.Deleting.Name);
    }

    [Fact(DisplayName = "PATCH file name should preserve the name when it exceeds the maximum length")]
    public async Task HandleAsync_Should_PreserveName_When_NameExceedsMaximumLength()
    {
        var userId = Guid.NewGuid();
        var file = CreateFile("Personal", userId);
        await SaveAsync(file);

        using var response = await SendAsync(
            file.Id.Value,
            new RenameFileRequest(new string('a', FileName.MaxLength + 1)),
            userId.ToString(),
            "file-manager.personal-files.update.own");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ReadAsync(file.Id.Value)).Name.ShouldBe("file.txt");
    }

    private static File CreateFile(string type, Guid userId)
    {
        return File.Create(
            FileName.Create("file.txt").Value,
            FileType.FromName(type),
            type == "Personal" ? UserId.Create(userId).Value : null).Value;
    }

    private async Task SaveAsync(File file, bool deleted = false)
    {
        await using var scope = Fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
        db.Add(file);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        if (deleted)
        {
            db.Remove(file);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }
    }

    private async Task<FileReadDbModel> ReadAsync(Guid id)
    {
        await using var scope = Fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FileManagerReadDbContext>();
        return await db.Set<FileReadDbModel>().IgnoreQueryFilters()
            .SingleAsync(file => file.Id == id, TestContext.Current.CancellationToken);
    }

    private async Task<HttpResponseMessage> SendAsync(Guid id, object body, string? subject, string permission)
    {
        using var request = new HttpRequestMessage(HttpMethod.Patch, $"{Route}/{id}/name");
        request.Content = JsonContent.Create(body);
        if (subject is not null)
        {
            request.Headers.Add(TestAuthenticationHandler.UserIdHeader, subject);
        }
        request.Headers.Add(TestAuthenticationHandler.PermissionsHeader, permission);
        return await Fixture.Client.SendAsync(request, TestContext.Current.CancellationToken);
    }
}
