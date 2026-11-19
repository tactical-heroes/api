using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using PANiXiDA.Core.Application.Messaging.Mediator;
using PANiXiDA.Core.Application.Persistence;
using PANiXiDA.Core.Application.Storage;
using PANiXiDA.Core.Application.Storage.Models;
using PANiXiDA.Core.Infrastructure.Messaging.Wolverine;
using PANiXiDA.Core.Infrastructure.Messaging.Wolverine.OutboxDispatcher;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.DeleteStored;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.ReconcileUploads;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.UpdateContent;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Scheduling.Options.FileUploadReconciliation;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Scheduling.Reconciliation;
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
        stored.PendingStorageKey.ShouldBeNull();
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
        var metadata = await storage.GetMetadataAsync(stored.StorageKey.Value, ct);
        metadata.ShouldNotBeNull();
        metadata.Size.ShouldBe(bytes.LongLength);
        await CleanupAsync(file.StorageKey);
        (await storage.GetMetadataAsync(file.StorageKey.Value, ct)).ShouldBeNull();
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

    [Fact(DisplayName = "Content update should accept identical bytes when content is identical")]
    public async Task HandleAsync_Should_AcceptIdenticalBytes_When_ContentIsIdentical()
    {
        var actorId = Guid.NewGuid();
        var file = await SaveAsync("Personal", actorId);

        using var response = await SendAsync(
            file.Id.Value, [1, 2, 3, 4], "text/plain", actorId, "file-manager.personal-files.update.own");

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var current = await ReadAsync(file.Id.Value);
        current.Status.ShouldBe(FileStatus.Ready);
        current.StorageKey.ShouldNotBe(file.StorageKey);
        current.PendingStorageKey.ShouldBeNull();
        current.Size!.Value.ShouldBe(4);
    }

    [Fact(DisplayName = "Reconciliation should activate uploaded content when upload response is lost")]
    public async Task Reconcile_Should_ActivateUploadedContent_When_UploadResponseIsLost()
    {
        var ct = TestContext.Current.CancellationToken;
        var file = await SaveAsync("Avatar", Guid.NewGuid());
        await using var scope = Fixture.CreateScope();
        var services = scope.ServiceProvider;
        var intercepted = new InterceptingFileStorage(
            services.GetRequiredService<IFileStorage>(), () => throw new IOException("Upload response was lost."));
        var handler = CreateHandler(services, intercepted);
        await using var content = new MemoryStream([7, 8, 9]);

        var exception = await Record.ExceptionAsync(() => handler.HandleAsync(
            new UpdateFileContentCommand(file.Id.Value, "image/png", 3, content), ct));

        exception.ShouldBeOfType<IOException>();
        var pending = await ReadAsync(file.Id.Value);
        pending.Status.ShouldBe(FileStatus.Updating);
        pending.StorageKey.ShouldBe(file.StorageKey);
        pending.PendingStorageKey.ShouldNotBeNull();
        await AgeUpdateAsync(file.Id.Value, expired: false);
        await ReconcileAsync();
        var current = await ReadAsync(file.Id.Value);
        current.Status.ShouldBe(FileStatus.Ready);
        current.StorageKey.ShouldBe(pending.PendingStorageKey);
        current.PendingStorageKey.ShouldBeNull();
        current.Size!.Value.ShouldBe(3);
        current.ContentType!.Value.ShouldBe("image/png");
        await CleanupAsync(file.StorageKey);
        await using var verification = Fixture.CreateScope();
        var storage = verification.ServiceProvider.GetRequiredService<IFileStorage>();
        (await storage.GetMetadataAsync(file.StorageKey.Value, ct)).ShouldBeNull();
        (await storage.GetMetadataAsync(current.StorageKey.Value, ct)).ShouldNotBeNull();
    }

    [Fact(DisplayName = "Reconciliation should restore old content when an abandoned attempt expires")]
    public async Task Reconcile_Should_RestoreOldContent_When_AnAbandonedAttemptExpires()
    {
        var ct = TestContext.Current.CancellationToken;
        var actorId = Guid.NewGuid();
        var file = await SaveAsync("Avatar", actorId, "in-progress");
        var candidate = file.PendingStorageKey!;

        await AgeUpdateAsync(file.Id.Value, expired: false);
        await ReconcileAsync();

        var pending = await ReadAsync(file.Id.Value);
        pending.Status.ShouldBe(FileStatus.Updating);
        pending.StorageKey.ShouldBe(file.StorageKey);
        await AgeUpdateAsync(file.Id.Value, expired: true);
        await ReconcileAsync();
        var restored = await ReadAsync(file.Id.Value);
        restored.Status.ShouldBe(FileStatus.Ready);
        restored.StorageKey.ShouldBe(file.StorageKey);
        restored.PendingStorageKey.ShouldBeNull();
        await using var scope = Fixture.CreateScope();
        var storage = scope.ServiceProvider.GetRequiredService<IFileStorage>();
        await using var late = new MemoryStream([9]);
        await storage.UploadAsync(candidate.Value, late, "text/plain", ct);
        await CleanupAsync(candidate);
        (await storage.GetMetadataAsync(candidate.Value, ct)).ShouldBeNull();
        (await storage.GetMetadataAsync(file.StorageKey.Value, ct)).ShouldNotBeNull();
        using var response = await SendAsync(
            file.Id.Value, [8], "text/plain", actorId, "file-manager.avatars.update");
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await ReadAsync(file.Id.Value)).StorageKey.ShouldNotBe(candidate);
    }

    [Fact(DisplayName = "Content update should reject a concurrent replacement when updating is persisted")]
    public async Task HandleAsync_Should_RejectConcurrentReplacement_When_UpdatingIsPersisted()
    {
        var ct = TestContext.Current.CancellationToken;
        var actorId = Guid.NewGuid();
        var file = await SaveAsync("Avatar", actorId);
        await using var scope = Fixture.CreateScope();
        var intercepted = new InterceptingFileStorage(
            scope.ServiceProvider.GetRequiredService<IFileStorage>(),
            async () =>
            {
                (await ReadAsync(file.Id.Value)).Status.ShouldBe(FileStatus.Updating);
                using var response = await SendAsync(
                    file.Id.Value, [0], "text/plain", actorId, "file-manager.avatars.update");
                response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
            });
        var handler = CreateHandler(scope.ServiceProvider, intercepted);
        await using var content = new MemoryStream([7, 8, 9]);

        var result = await handler.HandleAsync(
            new UpdateFileContentCommand(file.Id.Value, "text/plain", 3, content), ct);

        result.IsSuccess.ShouldBeTrue();
        var current = await ReadAsync(file.Id.Value);
        current.Status.ShouldBe(FileStatus.Ready);
        current.StorageKey.ShouldNotBe(file.StorageKey);
        current.Size!.Value.ShouldBe(3);
    }

    [Fact(DisplayName = "Content update should preserve active content when saving the new key conflicts")]
    public async Task HandleAsync_Should_PreserveActiveContent_When_SavingTheNewKeyConflicts()
    {
        var ct = TestContext.Current.CancellationToken;
        var file = await SaveAsync("Avatar", Guid.NewGuid());
        await using var scope = Fixture.CreateScope();
        var intercepted = new InterceptingFileStorage(
            scope.ServiceProvider.GetRequiredService<IFileStorage>(),
            async () =>
            {
                await using var other = Fixture.CreateScope();
                var db = other.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
                await db.Database.ExecuteSqlInterpolatedAsync(
                    $"UPDATE file_manager.files SET updated_at = now() WHERE id = {file.Id.Value}", ct);
            });
        var handler = CreateHandler(scope.ServiceProvider, intercepted);
        await using var content = new MemoryStream([7, 8, 9]);

        var exception = await Record.ExceptionAsync(() => handler.HandleAsync(
            new UpdateFileContentCommand(file.Id.Value, "text/plain", 3, content), ct));

        exception.ShouldBeOfType<PANiXiDA.Core.Application.Persistence.Errors.ConcurrencyException>();
        var current = await ReadAsync(file.Id.Value);
        current.StorageKey.ShouldBe(file.StorageKey);
        current.Status.ShouldBe(FileStatus.Updating);
        current.PendingStorageKey.ShouldNotBeNull();
        await using var verification = Fixture.CreateScope();
        var storage = verification.ServiceProvider.GetRequiredService<IFileStorage>();
        (await storage.GetMetadataAsync(file.StorageKey.Value, ct)).ShouldNotBeNull();
        await AgeUpdateAsync(file.Id.Value, expired: false);
        await ReconcileAsync();
        (await ReadAsync(file.Id.Value)).StorageKey.ShouldBe(current.PendingStorageKey);
    }

    private static UpdateFileContentHandler CreateHandler(IServiceProvider services, IFileStorage storage)
    {
        var dispatcher = services.GetRequiredKeyedService<IOutboxDispatcher>(typeof(FileManagerWriteDbContext));
        var cleanup = new FileContentCleanupScheduler(
            new WolverineScheduler(dispatcher),
            services.GetRequiredService<IOptions<FileUploadReconciliationOptions>>(),
            TimeProvider.System);

        return new UpdateFileContentHandler(
            services.GetRequiredService<IFilesRepository>(),
            storage,
            services.GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext)),
            new FileAccessPolicy(new UpdateUser()),
            cleanup);
    }

    private async Task CleanupAsync(FileStorageKey key)
    {
        await using var scope = Fixture.CreateScope();
        var result = await scope.ServiceProvider.GetRequiredService<IMediator>().SendAsync(
            new DeleteStoredFileCommand(key.Value, DateTimeOffset.UtcNow), TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
    }

    private async Task AgeUpdateAsync(Guid id, bool expired)
    {
        await using var scope = Fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
        var then = expired ? DateTime.UtcNow.AddDays(-2) : DateTime.UtcNow.AddMinutes(-20);
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE file_manager.files SET updated_at = {then} WHERE id = {id}",
            TestContext.Current.CancellationToken);
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
        await using var scope = Fixture.CreateScope();
        var storage = scope.ServiceProvider.GetRequiredService<IFileStorage>();
        await using var bytes = new MemoryStream([1, 2, 3, 4]);
        await storage.UploadAsync(file.StorageKey.Value, bytes, "text/plain", ct);
        if (condition == "deleting")
        {
            file.BeginDeletion();
        }
        if (condition == "in-progress")
        {
            file.BeginContentUpdate();
        }
        var db = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
        db.Add(file);
        await db.SaveChangesAsync(ct);
        if (condition == "deleted")
        {
            db.Remove(file);
            await db.SaveChangesAsync(ct);
        }
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
