using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.Core.Application.Storage;

using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Read.DbModels;
using PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.Create;
using PANiXiDA.TacticalHeroes.Testing.Storage;

namespace PANiXiDA.TacticalHeroes.FileManager.FunctionalTests.Presentation.Features.Files.Create;

public sealed class CreateFileEndpointTests(FunctionalTestFixture fixture) : FunctionalTestBase(fixture)
{
    private const string Route = "/api/v1/files";

    [Theory(DisplayName = "Create file endpoint should persist content and metadata when creation is permitted")]
    [InlineData("Personal", false, false, "file-manager.personal-files.create.own")]
    [InlineData("Personal", false, true, "file-manager.personal-files.create.own")]
    [InlineData("Personal", true, false, "file-manager.personal-files.create.all")]
    [InlineData("Personal", true, true, "file-manager.personal-files.create.all")]
    [InlineData("Avatar", false, false, "file-manager.avatars.create")]
    [InlineData("Avatar", false, true, "file-manager.avatars.create")]
    public async Task HandleAsync_Should_PersistContentAndMetadata_When_CreationIsPermitted(
        string type, bool foreignOwner, bool inFolder, string permission)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var actorId = Guid.NewGuid();
        Guid? ownerId = type == "Personal" ? foreignOwner ? Guid.NewGuid() : actorId : null;
        var folder = inFolder ? await AddFolderAsync(type, ownerId) : null;

        using var response = await SendAsync(type, ownerId, folder?.Id.Value, actorId, permission);

        response.StatusCode.ShouldBe(
            HttpStatusCode.Created,
            await response.Content.ReadAsStringAsync(cancellationToken));
        var created = await response.Content.ReadFromJsonAsync<CreateFileResponse>(cancellationToken);
        created.ShouldNotBeNull();
        created.Id.ShouldNotBe(Guid.Empty);
        response.Headers.Location.ShouldNotBeNull();
        var location = new Uri(Fixture.Client.BaseAddress!, response.Headers.Location);
        location.AbsolutePath.ShouldBe($"{Route}/{created.Id}");
        await using var scope = Fixture.CreateScope();
        var stored = await scope.ServiceProvider.GetRequiredService<FileManagerReadDbContext>()
            .Set<FileReadDbModel>()
            .SingleAsync(cancellationToken);
        stored.Id.ShouldBe(created.Id);
        stored.Name.ShouldBe("file.txt");
        stored.Type.ShouldBe(type);
        stored.UserId.ShouldBe(ownerId);
        stored.FolderId.ShouldBe(folder?.Id.Value);
        stored.Status.ShouldBe("Ready");
        stored.ContentType.ShouldBe("text/plain");
        stored.Size.ShouldBe(4);
        stored.StorageKey.ShouldBe(type == "Personal"
            ? $"personal/{ownerId:D}/{created.Id:D}" : $"avatar/{created.Id:D}");
        var storage = scope.ServiceProvider.GetRequiredService<IFileStorage>();
        await using var downloaded = await storage.DownloadAsync(stored.StorageKey, cancellationToken);
        await using var bytes = new MemoryStream();
        await downloaded.CopyToAsync(bytes, cancellationToken);
        bytes.ToArray().ShouldBe([1, 2, 3, 4]);
        var metadata = await storage.GetMetadataAsync(stored.StorageKey, cancellationToken);
        metadata.ShouldNotBeNull();
        metadata.ContentType.ShouldBe("text/plain");
        metadata.Size.ShouldBe(4);
    }

    [Theory(
        DisplayName = "Create file endpoint should deny creation when permission does not cover the requested scope")]
    [InlineData("Personal", false, "")]
    [InlineData("Personal", true, "file-manager.personal-files.create.own")]
    [InlineData("Personal", false, "file-manager.personal-files.read.all")]
    [InlineData("Personal", false, "file-manager.avatars.create")]
    [InlineData("Avatar", false, "file-manager.avatars.read")]
    [InlineData("Avatar", false, "file-manager.personal-files.create.all")]
    public async Task HandleAsync_Should_DenyCreation_When_PermissionDoesNotCoverTheRequestedScope(
        string type, bool foreignOwner, string permission)
    {
        var actorId = Guid.NewGuid();
        Guid? ownerId = type == "Personal" ? foreignOwner ? Guid.NewGuid() : actorId : null;

        using var response = await SendAsync(type, ownerId, null, actorId, permission);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        await AssertNoFilesAsync();
    }

    [Theory(DisplayName = "Create file endpoint should reject metadata when request is invalid")]
    [InlineData("missing-type")]
    [InlineData("unknown-type")]
    [InlineData("missing-file")]
    [InlineData("name")]
    [InlineData("content-type")]
    [InlineData("empty-content")]
    [InlineData("empty-owner")]
    [InlineData("empty-folder")]
    [InlineData("personal-owner")]
    [InlineData("avatar-owner")]
    public async Task HandleAsync_Should_RejectMetadata_When_RequestIsInvalid(string scenario)
    {
        var actorId = Guid.NewGuid();
        var type = scenario == "missing-type" ? null : scenario == "unknown-type" ? "Unknown"
            : scenario == "avatar-owner" ? "Avatar" : "Personal";
        Guid? ownerId = scenario == "personal-owner" ? null : scenario == "empty-owner" ? Guid.Empty : actorId;

        using var response = await SendAsync(
            type: type,
            ownerId: ownerId,
            folderId: scenario == "empty-folder" ? Guid.Empty : null,
            actorId: actorId,
            permission: "file-manager.personal-files.create.all,file-manager.avatars.create",
            name: scenario == "name" ? "../file.txt" : "file.txt",
            contentType: scenario == "content-type" ? "text/*" : "text/plain",
            emptyContent: scenario == "empty-content",
            includeFile: scenario != "missing-file");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        await AssertNoFilesAsync();
    }

    [Theory(DisplayName = "Create file endpoint should hide the folder when destination cannot be used")]
    [InlineData("missing")]
    [InlineData("deleted")]
    [InlineData("foreign")]
    [InlineData("type")]
    [InlineData("owner")]
    public async Task HandleAsync_Should_HideFolder_When_DestinationCannotBeUsed(string scenario)
    {
        var actorId = Guid.NewGuid();
        var folderType = scenario == "type" ? "Avatar" : "Personal";
        Guid? folderOwner = folderType == "Avatar" ? null
            : scenario is "foreign" or "owner" ? Guid.NewGuid() : actorId;
        var folder = await AddFolderAsync(folderType, folderOwner, deleted: scenario == "deleted");
        var permission = scenario == "foreign" ? "file-manager.personal-files.create.own"
            : "file-manager.personal-files.create.all,file-manager.avatars.create";

        using var response = await SendAsync(
            type: "Personal",
            ownerId: actorId,
            folderId: scenario == "missing" ? Guid.NewGuid() : folder.Id.Value,
            actorId: actorId,
            permission: permission);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        await AssertNoFilesAsync();
    }

    [Fact(DisplayName = "Create file endpoint should require authentication when caller is anonymous")]
    public async Task HandleAsync_Should_RequireAuthentication_When_CallerIsAnonymous()
    {
        using var response = await SendAsync("Avatar", null, null, null, "file-manager.avatars.create");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        await AssertNoFilesAsync();
    }

    private async Task<HttpResponseMessage> SendAsync(
        string? type,
        Guid? ownerId,
        Guid? folderId,
        Guid? actorId,
        string permission,
        string name = "file.txt",
        string contentType = "text/plain",
        bool emptyContent = false,
        bool includeFile = true)
    {
        using var form = new MultipartFormDataContent();
        using var file = new ByteArrayContent(emptyContent ? [] : [1, 2, 3, 4]);
        file.Headers.TryAddWithoutValidation("Content-Type", contentType);
        if (includeFile)
        {
            form.Add(file, "File", name);
        }
        using var fileType = type is not null ? new StringContent(type) : null;
        if (fileType is not null)
        {
            form.Add(fileType, "Type");
        }
        using var owner = ownerId is { } userId ? new StringContent(userId.ToString()) : null;
        if (owner is not null)
        {
            form.Add(owner, "UserId");
        }
        using var folder = folderId is { } id ? new StringContent(id.ToString()) : null;
        if (folder is not null)
        {
            form.Add(folder, "FolderId");
        }
        using var message = new HttpRequestMessage(HttpMethod.Post, Route) { Content = form };
        if (actorId is { } actor)
        {
            message.Headers.Add(TestAuthenticationHandler.UserIdHeader, actor.ToString());
        }
        message.Headers.Add(TestAuthenticationHandler.PermissionsHeader, permission);
        return await Fixture.Client.SendAsync(message, TestContext.Current.CancellationToken);
    }

    private async Task<Folder> AddFolderAsync(string type, Guid? ownerId, bool deleted = false)
    {
        var folder = Folder.Create(
            FolderName.Create("Folder").Value, FileType.FromName(type), UserId.Create(ownerId).Value).Value;
        await using var scope = Fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
        db.Add(folder);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        if (deleted)
        {
            db.Remove(folder);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }
        return folder;
    }

    private async Task AssertNoFilesAsync()
    {
        await using var scope = Fixture.CreateScope();
        var count = await scope.ServiceProvider.GetRequiredService<FileManagerReadDbContext>()
            .Set<FileReadDbModel>()
            .IgnoreQueryFilters()
            .CountAsync(TestContext.Current.CancellationToken);
        count.ShouldBe(0);
        using var s3 = Fixture.Storage.CreateClient();
        var stored = await s3.ListObjectsV2Async(
            new Amazon.S3.Model.ListObjectsV2Request { BucketName = S3TestStorage.BucketName },
            TestContext.Current.CancellationToken);
        (stored.S3Objects?.Count ?? 0).ShouldBe(0);
    }
}
