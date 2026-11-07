using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.Core.Application.Messaging.Mediator;
using PANiXiDA.Core.Application.Persistence;
using PANiXiDA.Core.Application.Persistence.Errors;
using PANiXiDA.Core.Application.Storage;
using PANiXiDA.Core.Application.Storage.Models;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.ReconcileUploads;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.UpdateContent;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Entities.FileContents.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.GetDownloadUrl;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.FunctionalTests.Presentation.Features.Files.UpdateContent;

[Collection(ManualReconciliationTestCollectionDefinition.Name)]
public sealed class UpdateFileContentEndpointTests(
    ManualReconciliationTestFixture fixture) : FunctionalTestBase(fixture)
{
    private const string Route = "/api/v1/files";

    [Theory(DisplayName = "PUT file content should preserve file identity when content is replaced")]
    [InlineData("Personal", true, "file-manager.personal-files.update.own")]
    [InlineData("Personal", false, "file-manager.personal-files.update.all")]
    [InlineData("Avatar", false, "file-manager.avatars.update")]
    public async Task HandleAsync_Should_PreserveFileIdentity_When_ContentIsReplaced(
        string type, bool own, string permission)
    {
        var ct = TestContext.Current.CancellationToken;
        var actorId = Guid.NewGuid();
        var file = await SaveAsync(type, own ? actorId : Guid.NewGuid());
        byte[] bytes = [5, 6, 7, 8, 9];

        using var response = await SendAsync(file.Id.Value, bytes, "application/octet-stream", actorId, permission);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var stored = await ReadAsync(file.Id.Value);
        stored.Name.ShouldBe(file.Name);
        stored.Type.ShouldBe(file.Type);
        stored.UserId.ShouldBe(file.UserId);
        stored.FolderId.ShouldBe(file.FolderId);
        stored.Status.Name.ShouldBe("Ready");
        stored.ContentType!.Value.ShouldBe("application/octet-stream");
        stored.Size!.Value.ShouldBe(bytes.Length);
        stored.StorageKey.ShouldNotBe(file.StorageKey);
        stored.UnusedContents.ShouldHaveSingleItem().StorageKey.ShouldBe(file.StorageKey);
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{Route}/{file.Id.Value}/download-url");
        request.Headers.Add(TestAuthenticationHandler.UserIdHeader, actorId.ToString());
        request.Headers.Add(
            TestAuthenticationHandler.PermissionsHeader,
            type == "Personal" ? "file-manager.personal-files.read.all" : "file-manager.avatars.read");
        using var urlResponse = await Fixture.Client.SendAsync(request, ct);
        urlResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var url = await urlResponse.Content.ReadFromJsonAsync<GetFileDownloadUrlResponse>(ct);
        url.ShouldNotBeNull();
        using var client = new HttpClient();
        using var downloaded = await client.GetAsync(url.Url, ct);
        (await downloaded.Content.ReadAsByteArrayAsync(ct)).ShouldBe(bytes);
        downloaded.Content.Headers.ContentDisposition!.FileNameStar.ShouldBe("original.txt");
        await ReconcileAsync();
        await using var scope = Fixture.CreateScope();
        var storage = scope.ServiceProvider.GetRequiredService<IFileStorage>();
        (await storage.GetMetadataAsync(file.StorageKey.Value, ct)).ShouldBeNull();
        (await storage.GetMetadataAsync(stored.StorageKey.Value, ct)).ShouldNotBeNull();
    }

    [Theory(DisplayName = "PUT file content should deny changes when update access is unavailable")]
    [InlineData("Personal", false, "file-manager.personal-files.update.own", 404)]
    [InlineData("Personal", true, "file-manager.personal-files.create.own", 404)]
    [InlineData("Personal", true, "file-manager.personal-files.read.all", 404)]
    [InlineData("Avatar", true, "file-manager.personal-files.update.all", 404)]
    [InlineData("Personal", true, "anonymous", 401)]
    public async Task HandleAsync_Should_DenyChanges_When_UpdateAccessIsUnavailable(
        string type, bool own, string permission, int status)
    {
        var actorId = Guid.NewGuid();
        var file = await SaveAsync(type, own ? actorId : Guid.NewGuid());

        using var response = await SendAsync(
            file.Id.Value,
            [9],
            "text/plain",
            permission == "anonymous" ? null : actorId,
            permission);

        ((int)response.StatusCode).ShouldBe(status);
        var stored = await ReadAsync(file.Id.Value);
        stored.StorageKey.ShouldBe(file.StorageKey);
        stored.UnusedContents.ShouldBeEmpty();
        stored.Size!.Value.ShouldBe(4);
    }

    [Theory(DisplayName = "PUT file content should preserve current content when upload cannot start")]
    [InlineData("pending", 409)]
    [InlineData("deleting", 409)]
    [InlineData("in-progress", 409)]
    [InlineData("deleted", 404)]
    [InlineData("missing", 404)]
    [InlineData("id", 400)]
    [InlineData("empty", 400)]
    [InlineData("content-type", 400)]
    public async Task HandleAsync_Should_PreserveCurrentContent_When_UploadCannotStart(string condition, int status)
    {
        var actorId = Guid.NewGuid();
        var file = await SaveAsync("Personal", actorId, condition);

        using var response = await SendAsync(
            condition == "missing" ? Guid.NewGuid() : condition == "id" ? Guid.Empty : file.Id.Value,
            condition == "empty" ? [] : [9],
            condition == "content-type" ? "text/*" : "text/plain",
            actorId,
            "file-manager.personal-files.update.own");

        ((int)response.StatusCode).ShouldBe(status);
        (await ReadAsync(file.Id.Value)).StorageKey.ShouldBe(file.StorageKey);
    }

    [Fact(DisplayName = "Content reconciliation should remove abandoned uploads when upload expires")]
    public async Task Reconcile_Should_RemoveAbandonedUploads_When_UploadExpires()
    {
        var ct = TestContext.Current.CancellationToken;
        var file = await SaveAsync("Avatar", Guid.NewGuid());
        string pendingKey;
        await using (var scope = Fixture.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
            var stored = await db.Set<File>().SingleAsync(item => item.Id == file.Id, ct);
            var pending = stored.BeginContentUpload(FileContentTimestamp.Create(DateTimeOffset.UtcNow.AddMinutes(-20)))
                .Value;
            pendingKey = pending.StorageKey.Value;
            db.Update(stored);
            await db.SaveChangesAsync(ct);
            await using var bytes = new MemoryStream([9]);
            await scope.ServiceProvider.GetRequiredService<IFileStorage>()
                .UploadAsync(pendingKey, bytes, "text/plain", ct);
        }

        await ReconcileAsync();

        var current = await ReadAsync(file.Id.Value);
        current.StorageKey.ShouldBe(file.StorageKey);
        current.UnusedContents.ShouldHaveSingleItem().DeletionStartedAt.ShouldNotBeNull();
        await using var verification = Fixture.CreateScope();
        var storage = verification.ServiceProvider.GetRequiredService<IFileStorage>();
        (await storage.GetMetadataAsync(pendingKey, ct)).ShouldBeNull();
        (await storage.GetMetadataAsync(file.StorageKey.Value, ct)).ShouldNotBeNull();
        await using var late = new MemoryStream([8]);
        await storage.UploadAsync(pendingKey, late, "text/plain", ct);
        await ReconcileAsync();
        (await storage.GetMetadataAsync(pendingKey, ct)).ShouldBeNull();
    }

    [Fact(DisplayName = "Content reconciliation should remove cleanup records when recheck window expires")]
    public async Task Reconcile_Should_RemoveCleanupRecords_When_RecheckWindowExpires()
    {
        var ct = TestContext.Current.CancellationToken;
        var file = await SaveAsync("Avatar", Guid.NewGuid());
        await using (var scope = Fixture.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
            var stored = await db.Set<File>().SingleAsync(item => item.Id == file.Id, ct);
            var then = FileContentTimestamp.Create(DateTimeOffset.UtcNow.AddDays(-2));
            var pending = stored.BeginContentUpload(then).Value;
            stored.BeginContentDeletion(pending.Id, then);
            db.Update(stored);
            await db.SaveChangesAsync(ct);
        }

        await ReconcileAsync();

        var current = await ReadAsync(file.Id.Value);
        current.UnusedContents.ShouldBeEmpty();
        current.StorageKey.ShouldBe(file.StorageKey);
    }

    [Fact(DisplayName = "Content reconciliation should clean abandoned objects when the file is soft deleted")]
    public async Task Reconcile_Should_CleanAbandonedObjects_When_TheFileIsSoftDeleted()
    {
        var ct = TestContext.Current.CancellationToken;
        var file = await SaveAsync("Avatar", Guid.NewGuid());
        string pendingKey;
        await using (var scope = Fixture.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
            var stored = await db.Set<File>().SingleAsync(item => item.Id == file.Id, ct);
            var then = FileContentTimestamp.Create(DateTimeOffset.UtcNow.AddMinutes(-20));
            pendingKey = stored.BeginContentUpload(then).Value.StorageKey.Value;
            db.Update(stored);
            await db.SaveChangesAsync(ct);
            db.Remove(stored);
            await db.SaveChangesAsync(ct);
            await using var bytes = new MemoryStream([9]);
            await scope.ServiceProvider.GetRequiredService<IFileStorage>().UploadAsync(
                pendingKey, bytes, "text/plain", ct);
        }

        await ReconcileAsync();

        await using var verification = Fixture.CreateScope();
        var write = verification.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
        (await write.Set<File>().AnyAsync(item => item.Id == file.Id, ct)).ShouldBeFalse();
        var storage = verification.ServiceProvider.GetRequiredService<IFileStorage>();
        (await storage.GetMetadataAsync(pendingKey, ct)).ShouldBeNull();
        (await storage.GetMetadataAsync(file.StorageKey.Value, ct)).ShouldBeNull();
    }

    [Fact(DisplayName = "Content update should preserve committed metadata when another writer changes the file")]
    public async Task HandleAsync_Should_PreserveCommittedMetadata_When_AnotherWriterChangesTheFile()
    {
        var ct = TestContext.Current.CancellationToken;
        var file = await SaveAsync("Avatar", Guid.NewGuid());
        await using var scope = Fixture.CreateScope();
        var services = scope.ServiceProvider;
        var storage = services.GetRequiredService<IFileStorage>();
        var intercepted = new InterceptingFileStorage(
            storage,
            async () =>
            {
                await using var other = Fixture.CreateScope();
                var db = other.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
                var stored = await db.Set<File>().SingleAsync(item => item.Id == file.Id, ct);
                stored.Rename(FileName.Create("renamed.txt").Value);
                db.Update(stored);
                await db.SaveChangesAsync(ct);
            });
        var handler = new UpdateFileContentHandler(
            services.GetRequiredService<IFilesRepository>(),
            intercepted,
            services.GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext)),
            new FileAccessPolicy(new UpdateUser()),
            TimeProvider.System);
        await using var content = new MemoryStream([7, 8, 9]);

        var exception = await Record.ExceptionAsync(() => handler.HandleAsync(
            new UpdateFileContentCommand(file.Id.Value, "text/plain", 3, content), ct));

        exception.ShouldBeOfType<ConcurrencyException>();
        var current = await ReadAsync(file.Id.Value);
        current.Name.Value.ShouldBe("renamed.txt");
        current.StorageKey.ShouldBe(file.StorageKey);
        current.Size!.Value.ShouldBe(4);
        current.UnusedContents.ShouldHaveSingleItem().DeletionStartedAt.ShouldBeNull();
    }

    private async Task<File> SaveAsync(string type, Guid ownerId, string condition = "ready")
    {
        var ct = TestContext.Current.CancellationToken;
        var file = File.Create(
            FileName.Create("original.txt").Value,
            FileType.FromName(type),
            type == "Personal" ? UserId.Create(ownerId).Value : null).Value;
        if (condition != "pending")
        {
            file.CompleteUpload(FileContentType.Create("text/plain").Value, FileSize.Create(4).Value);
        }
        if (condition == "deleting")
        {
            file.BeginDeletion();
        }
        if (condition == "in-progress")
        {
            file.BeginContentUpload(FileContentTimestamp.Create(DateTimeOffset.UtcNow));
        }
        await using var scope = Fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
        db.Add(file);
        await db.SaveChangesAsync(ct);
        if (condition == "deleted")
        {
            db.Remove(file);
            await db.SaveChangesAsync(ct);
        }
        await using var bytes = new MemoryStream([1, 2, 3, 4]);
        await scope.ServiceProvider.GetRequiredService<IFileStorage>().UploadAsync(
            file.StorageKey.Value, bytes, "text/plain", ct);
        return file;
    }

    private async Task<File> ReadAsync(Guid id)
    {
        await using var scope = Fixture.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>()
            .Set<File>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .SingleAsync(
                file => file.Id == PANiXiDA.TacticalHeroes.FileManager.Domain.Files.FileId.Create(id).Value,
                TestContext.Current.CancellationToken);
    }

    private async Task ReconcileAsync()
    {
        await using var scope = Fixture.CreateScope();
        var result = await scope.ServiceProvider.GetRequiredService<IMediator>().SendAsync(
            new ReconcileFileUploadsCommand(), TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
    }

    private async Task<HttpResponseMessage> SendAsync(
        Guid id, byte[] bytes, string contentType, Guid? actorId, string permission)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, $"{Route}/{id}/content");
        using var form = new MultipartFormDataContent();
        using var content = new ByteArrayContent(bytes);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        form.Add(content, "File", "ignored-name.bin");
        request.Content = form;
        if (actorId is { } value)
        {
            request.Headers.Add(TestAuthenticationHandler.UserIdHeader, value.ToString());
        }
        request.Headers.Add(TestAuthenticationHandler.PermissionsHeader, permission);
        return await Fixture.Client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    private sealed class UpdateUser : PANiXiDA.Core.Application.Authentication.Abstractions.ICurrentUser
    {
        public bool IsAuthenticated => true;
        public Guid? UserId => null;
        public string? UserName => null;
        public string? AccessToken => null;
        public IReadOnlyCollection<string> Roles => [];

        public bool TryGetClaimValue<T>(
            string claimType,
            [System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out T value)
            where T : IParsable<T>
        {
            value = default;
            return false;
        }

        public bool HasPermission(string permission) => true;
    }

    private sealed class InterceptingFileStorage(IFileStorage inner, Func<Task> afterUpload) : IFileStorage
    {
        public async Task UploadAsync(
            string key,
            Stream content,
            string contentType,
            CancellationToken cancellationToken)
        {
            await inner.UploadAsync(key, content, contentType, cancellationToken);
            await afterUpload();
        }

        public Task<Stream> DownloadAsync(string key, CancellationToken cancellationToken)
            => inner.DownloadAsync(key, cancellationToken);

        public Task<FileMetadata?> GetMetadataAsync(string key, CancellationToken cancellationToken)
            => inner.GetMetadataAsync(key, cancellationToken);

        public Task DeleteAsync(string key, CancellationToken cancellationToken)
            => inner.DeleteAsync(key, cancellationToken);

        public Task<PresignedUploadUrl> GetPresignedUploadUrlAsync(
            string key, string contentType, long size, CancellationToken cancellationToken)
            => inner.GetPresignedUploadUrlAsync(key, contentType, size, cancellationToken);

        public Task<PresignedDownloadUrl> GetPresignedDownloadUrlAsync(
            string key, string fileName, string contentType, CancellationToken cancellationToken)
            => inner.GetPresignedDownloadUrlAsync(key, fileName, contentType, cancellationToken);
    }
}
