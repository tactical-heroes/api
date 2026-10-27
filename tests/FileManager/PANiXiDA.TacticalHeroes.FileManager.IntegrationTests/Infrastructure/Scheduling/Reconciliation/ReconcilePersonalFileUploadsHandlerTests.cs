using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

using Npgsql;

using PANiXiDA.Core.Application.Persistence.Errors;
using PANiXiDA.Core.Application.Storage;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.ReconcilePersonalUploads;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.DependencyInjection;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Read.DbModels;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Write;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Scheduling.Options.FileUploadReconciliation;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.IntegrationTests.Infrastructure.Scheduling.Reconciliation;

public sealed class ReconcilePersonalFileUploadsHandlerTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact(DisplayName = "Reconciliation should recover ready metadata when uploaded object exists")]
    public async Task HandleAsync_Should_RecoverReadyMetadata_When_UploadedObjectExists()
    {
        var file = await SaveFileAsync();
        await UploadAsync(file);
        await using var scope = Fixture.CreateScope();

        await CreateHandler(scope.ServiceProvider).HandleAsync(
            new ReconcilePersonalFileUploadsCommand(), TestContext.Current.CancellationToken);

        var stored = await ReadFileAsync(file.Id);
        stored.Status.ShouldBe("Ready");
        stored.Size.ShouldBe(4);
        stored.ContentType.ShouldBe("text/plain");
        stored.StorageKey.ShouldBe(file.StorageKey.Value);
    }

    [Fact(DisplayName = "Reconciliation should retry pending content when object arrives after an earlier scan")]
    public async Task HandleAsync_Should_RetryPendingContent_When_ObjectArrivesAfterAnEarlierScan()
    {
        var file = await SaveFileAsync();
        await using var scope = Fixture.CreateScope();
        var job = CreateHandler(scope.ServiceProvider);
        await job.HandleAsync(new ReconcilePersonalFileUploadsCommand(), TestContext.Current.CancellationToken);
        (await ReadFileAsync(file.Id)).Status.ShouldBe("PendingUpload");

        await UploadAsync(file);
        await job.HandleAsync(new ReconcilePersonalFileUploadsCommand(), TestContext.Current.CancellationToken);

        (await ReadFileAsync(file.Id)).Status.ShouldBe("Ready");
    }

    [Theory(DisplayName = "Reconciliation should leave unrelated records unchanged when file is ineligible")]
    [InlineData("recent")]
    [InlineData("avatar")]
    [InlineData("ready")]
    public async Task HandleAsync_Should_LeaveUnrelatedRecordsUnchanged_When_FileIsIneligible(string condition)
    {
        var file = await SaveFileAsync(condition);
        await UploadAsync(file);
        var original = await ReadFileAsync(file.Id);
        await using var scope = Fixture.CreateScope();

        await CreateHandler(scope.ServiceProvider).HandleAsync(
            new ReconcilePersonalFileUploadsCommand(), TestContext.Current.CancellationToken);

        var stored = await ReadFileAsync(file.Id);
        stored.Status.ShouldBe(original.Status);
        stored.Size.ShouldBe(original.Size);
        stored.ContentType.ShouldBe(original.ContentType);
    }

    [Fact(DisplayName = "Reconciliation should visit every page when earlier files are still missing")]
    public async Task HandleAsync_Should_VisitEveryPage_When_EarlierFilesAreStillMissing()
    {
        var files = new[] { await SaveFileAsync(), await SaveFileAsync(), await SaveFileAsync() };
        var ordered = files.OrderBy(file => file.Id.Value).ToArray();
        await UploadAsync(ordered[1]);
        await UploadAsync(ordered[2]);
        await using var scope = Fixture.CreateScope();

        await CreateHandler(scope.ServiceProvider, batchSize: 1)
            .HandleAsync(new ReconcilePersonalFileUploadsCommand(), TestContext.Current.CancellationToken);

        (await ReadFileAsync(ordered[0].Id)).Status.ShouldBe("PendingUpload");
        (await ReadFileAsync(ordered[1].Id)).Status.ShouldBe("Ready");
        (await ReadFileAsync(ordered[2].Id)).Status.ShouldBe("Ready");
    }

    [Fact(DisplayName = "Reconciliation should preserve pending state when storage is unavailable")]
    public async Task HandleAsync_Should_PreservePendingState_When_StorageIsUnavailable()
    {
        var file = await SaveFileAsync();
        using var client = Fixture.Storage.CreateClient();
        await client.DeleteBucketAsync(
            new Amazon.S3.Model.DeleteBucketRequest
            {
                BucketName = PANiXiDA.TacticalHeroes.Testing.Storage.S3TestStorage.BucketName
            },
            TestContext.Current.CancellationToken);
        FileReadDbModel stored;
        try
        {
            await using var scope = Fixture.CreateScope();

            await CreateHandler(scope.ServiceProvider).HandleAsync(
                new ReconcilePersonalFileUploadsCommand(), TestContext.Current.CancellationToken);

            stored = await ReadFileAsync(file.Id);
        }
        finally
        {
            await client.PutBucketAsync(
                new Amazon.S3.Model.PutBucketRequest
                {
                    BucketName = PANiXiDA.TacticalHeroes.Testing.Storage.S3TestStorage.BucketName
                },
                TestContext.Current.CancellationToken);
        }

        stored.Status.ShouldBe("PendingUpload");
    }

    [Fact(DisplayName = "Reconciliation should propagate cancellation when cancellation is requested")]
    public async Task HandleAsync_Should_PropagateCancellation_When_CancellationIsRequested()
    {
        await using var scope = Fixture.CreateScope();
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        await cancellation.CancelAsync();

        var exception = await Record.ExceptionAsync(() =>
            CreateHandler(scope.ServiceProvider).HandleAsync(
                new ReconcilePersonalFileUploadsCommand(), cancellation.Token));

        exception.ShouldBeAssignableTo<OperationCanceledException>();
    }

    [Theory(DisplayName = "Reconciliation should preserve pending state when stored metadata is invalid")]
    [InlineData(0, "text/plain")]
    [InlineData(4, "text/*")]
    public async Task HandleAsync_Should_KeepPending_When_StoredMetadataIsInvalid(int size, string contentType)
    {
        var file = await SaveFileAsync();
        await using var scope = Fixture.CreateScope();
        await using var content = new MemoryStream(new byte[size]);
        await scope.ServiceProvider.GetRequiredService<IFileStorage>()
            .UploadAsync(file.StorageKey.Value, content, contentType, TestContext.Current.CancellationToken);

        await CreateHandler(scope.ServiceProvider).HandleAsync(
            new ReconcilePersonalFileUploadsCommand(), TestContext.Current.CancellationToken);

        var stored = await ReadFileAsync(file.Id);
        stored.Status.ShouldBe("PendingUpload");
        stored.Size.ShouldBeNull();
        stored.ContentType.ShouldBeNull();
    }

    [Fact(DisplayName = "Reconciliation should stop and retry when file changes before save")]
    public async Task HandleAsync_Should_StopAndRetry_When_FileChangesBeforeSave()
    {
        var changedFile = await SaveFileAsync();
        var otherFile = await SaveFileAsync();
        await UploadAsync(changedFile);
        await UploadAsync(otherFile);
        var settings = Fixture.Storage.GetConfiguration(nameof(FileManager));
        settings["ConnectionStrings:PostgreSqlConnectionString"] = Fixture.ConnectionString;
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        var services = new ServiceCollection();
        services.AddInfrastructure(configuration);
        var interceptor = new ChangeFileBeforeSaveInterceptor(changedFile.Id.Value, Fixture.ConnectionString);
        services.AddSingleton<IInterceptor>(interceptor);
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var reconciler = CreateHandler(scope.ServiceProvider, batchSize: 1);

        var exception = await Record.ExceptionAsync(() => reconciler.HandleAsync(
            new ReconcilePersonalFileUploadsCommand(), TestContext.Current.CancellationToken));
        var pending = await ReadFileAsync(changedFile.Id);
        await using var retryScope = provider.CreateAsyncScope();
        await CreateHandler(retryScope.ServiceProvider, batchSize: 1)
            .HandleAsync(new ReconcilePersonalFileUploadsCommand(), TestContext.Current.CancellationToken);
        var completed = await ReadFileAsync(otherFile.Id);
        var retried = await ReadFileAsync(changedFile.Id);

        exception.ShouldBeOfType<ConcurrencyException>();
        pending.Status.ShouldBe("PendingUpload");
        pending.Name.ShouldBe("renamed.txt");
        pending.Size.ShouldBeNull();
        completed.Status.ShouldBe("Ready");
        retried.Status.ShouldBe("Ready");
        retried.Name.ShouldBe("renamed.txt");
        retried.Size.ShouldBe(4);
    }

    [Fact(DisplayName = "Reconciliation should delete an expired upload when stored content is missing")]
    public async Task HandleAsync_Should_DeleteExpiredUpload_When_StoredContentIsMissing()
    {
        var file = await SaveFileAsync("expired");
        await using var scope = Fixture.CreateScope();

        await CreateHandler(scope.ServiceProvider).HandleAsync(
            new ReconcilePersonalFileUploadsCommand(), TestContext.Current.CancellationToken);

        var stored = await ReadFileAsync(file.Id);
        stored.Status.ShouldBe("Deleted");
        stored.Size.ShouldBeNull();
        stored.DeletedAt.ShouldBeNull();
    }

    [Fact(DisplayName = "Reconciliation should recover an expired upload when stored content is valid")]
    public async Task HandleAsync_Should_RecoverExpiredUpload_When_StoredContentIsValid()
    {
        var file = await SaveFileAsync("expired");
        await UploadAsync(file);
        await using var scope = Fixture.CreateScope();

        await CreateHandler(scope.ServiceProvider).HandleAsync(
            new ReconcilePersonalFileUploadsCommand(), TestContext.Current.CancellationToken);

        (await ReadFileAsync(file.Id)).Status.ShouldBe("Ready");
        var metadata = await scope.ServiceProvider.GetRequiredService<IFileStorage>()
            .GetMetadataAsync(file.StorageKey.Value, TestContext.Current.CancellationToken);
        metadata.ShouldNotBeNull();
    }

    [Theory(DisplayName = "Reconciliation should finish persisted deletion when stored content may exist")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_FinishPersistedDeletion_When_StoredContentMayExist(bool contentExists)
    {
        var file = await SaveFileAsync("deleting");
        if (contentExists)
        {
            await UploadAsync(file);
        }

        await using var scope = Fixture.CreateScope();

        await CreateHandler(scope.ServiceProvider).HandleAsync(
            new ReconcilePersonalFileUploadsCommand(), TestContext.Current.CancellationToken);

        (await ReadFileAsync(file.Id)).Status.ShouldBe("Deleted");
        var metadata = await scope.ServiceProvider.GetRequiredService<IFileStorage>()
            .GetMetadataAsync(file.StorageKey.Value, TestContext.Current.CancellationToken);
        metadata.ShouldBeNull();
    }

    [Fact(DisplayName = "Reconciliation should retry deletion when storage bucket becomes available again")]
    public async Task HandleAsync_Should_RetryDeletion_When_StorageBucketBecomesAvailableAgain()
    {
        var file = await SaveFileAsync("expired");
        using var client = Fixture.Storage.CreateClient();
        await client.DeleteBucketAsync(
            new Amazon.S3.Model.DeleteBucketRequest
            {
                BucketName = PANiXiDA.TacticalHeroes.Testing.Storage.S3TestStorage.BucketName
            },
            TestContext.Current.CancellationToken);
        try
        {
            await using var scope = Fixture.CreateScope();

            await CreateHandler(scope.ServiceProvider).HandleAsync(
                new ReconcilePersonalFileUploadsCommand(), TestContext.Current.CancellationToken);

            (await ReadFileAsync(file.Id)).Status.ShouldBe("Deleting");
        }
        finally
        {
            await client.PutBucketAsync(
                new Amazon.S3.Model.PutBucketRequest
                {
                    BucketName = PANiXiDA.TacticalHeroes.Testing.Storage.S3TestStorage.BucketName
                },
                TestContext.Current.CancellationToken);
        }

        await using var retryScope = Fixture.CreateScope();
        await CreateHandler(retryScope.ServiceProvider).HandleAsync(
            new ReconcilePersonalFileUploadsCommand(), TestContext.Current.CancellationToken);

        (await ReadFileAsync(file.Id)).Status.ShouldBe("Deleted");
    }

    [Fact(DisplayName = "Reconciliation should remove a late upload when original uploader loses a version race")]
    public async Task HandleAsync_Should_RemoveLateUpload_When_OriginalUploaderLosesAVersionRace()
    {
        var file = await SaveFileAsync("expired");
        await using var uploadScope = Fixture.CreateScope();
        var uploadRepository = uploadScope.ServiceProvider.GetRequiredService<IFilesRepository>();
        var original = await uploadRepository.GetByIdAsync(file.Id, TestContext.Current.CancellationToken);
        await using (var deletionScope = Fixture.CreateScope())
        {
            await CreateHandler(deletionScope.ServiceProvider).HandleAsync(
                new ReconcilePersonalFileUploadsCommand(), TestContext.Current.CancellationToken);
        }

        await UploadAsync(file);
        original!.CompleteUpload(FileContentType.Create("text/plain").Value, FileSize.Create(4).Value);
        uploadRepository.Update(original);
        var unitOfWork = uploadScope.ServiceProvider
            .GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext));
        var exception = await Record.ExceptionAsync(() => unitOfWork.SaveChangesAsync(
            TestContext.Current.CancellationToken));
        await using var cleanupScope = Fixture.CreateScope();

        await CreateHandler(cleanupScope.ServiceProvider).HandleAsync(
            new ReconcilePersonalFileUploadsCommand(), TestContext.Current.CancellationToken);

        exception.ShouldBeOfType<ConcurrencyException>();
        (await ReadFileAsync(file.Id)).Status.ShouldBe("Deleted");
        var metadata = await cleanupScope.ServiceProvider.GetRequiredService<IFileStorage>()
            .GetMetadataAsync(file.StorageKey.Value, TestContext.Current.CancellationToken);
        metadata.ShouldBeNull();
    }

    [Fact(DisplayName = "Reconciliation should retry finalization when saving after storage deletion conflicts")]
    public async Task HandleAsync_Should_RetryFinalization_When_SavingAfterStorageDeletionConflicts()
    {
        var file = await SaveFileAsync("deleting");
        await UploadAsync(file);
        var settings = Fixture.Storage.GetConfiguration(nameof(FileManager));
        settings["ConnectionStrings:PostgreSqlConnectionString"] = Fixture.ConnectionString;
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        var services = new ServiceCollection();
        services.AddInfrastructure(configuration);
        services.AddSingleton<IInterceptor>(new ChangeFileBeforeSaveInterceptor(
            fileId: file.Id.Value,
            connectionString: Fixture.ConnectionString,
            duringDeletion: true));
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();

        var exception = await Record.ExceptionAsync(() => CreateHandler(scope.ServiceProvider).HandleAsync(
            new ReconcilePersonalFileUploadsCommand(), TestContext.Current.CancellationToken));
        var pending = await ReadFileAsync(file.Id);
        var metadata = await scope.ServiceProvider.GetRequiredService<IFileStorage>()
            .GetMetadataAsync(file.StorageKey.Value, TestContext.Current.CancellationToken);
        await using var retryScope = provider.CreateAsyncScope();
        await CreateHandler(retryScope.ServiceProvider).HandleAsync(
            new ReconcilePersonalFileUploadsCommand(), TestContext.Current.CancellationToken);

        exception.ShouldBeOfType<ConcurrencyException>();
        pending.Status.ShouldBe("Deleting");
        metadata.ShouldBeNull();
        (await ReadFileAsync(file.Id)).Status.ShouldBe("Deleted");
    }

    private static ReconcilePersonalFileUploadsHandler CreateHandler(
        IServiceProvider services,
        int batchSize = 100)
    {
        var options = Microsoft.Extensions.Options.Options.Create(new FileUploadReconciliationOptions
        {
            BatchSize = batchSize
        });
        var filesRepository = new FilesRepository(
            dbContext: services.GetRequiredService<FileManagerWriteDbContext>(),
            options: options,
            timeProvider: TimeProvider.System);

        return new ReconcilePersonalFileUploadsHandler(
            filesRepository: filesRepository,
            fileStorage: services.GetRequiredService<IFileStorage>(),
            unitOfWork: services.GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext)),
            logger: NullLogger<ReconcilePersonalFileUploadsHandler>.Instance);
    }

    private async Task<File> SaveFileAsync(string condition = "pending")
    {
        var file = File.Create(
            FileName.Create("file.txt").Value,
            condition == "avatar" ? FileType.Avatar : FileType.Personal,
            condition == "avatar" ? null : UserId.Create(Guid.NewGuid()).Value).Value;
        if (condition is "ready" or "deleting" or "deleted")
        {
            file.CompleteUpload(FileContentType.Create("text/plain").Value, FileSize.Create(8).Value);
        }

        if (condition is "deleting" or "deleted")
        {
            file.BeginDeletion();
        }

        if (condition == "deleted")
        {
            file.CompleteDeletion();
        }

        await using var scope = Fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
        context.Add(file);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        if (condition != "recent")
        {
            var createdAt = condition == "expired" ? DateTime.UnixEpoch : DateTime.UtcNow.AddMinutes(-30);
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE file_manager.files SET created_at = {createdAt} WHERE id = {file.Id.Value}",
                TestContext.Current.CancellationToken);
        }

        return file;
    }

    private async Task UploadAsync(File file)
    {
        await using var scope = Fixture.CreateScope();
        await using var content = new MemoryStream([1, 2, 3, 4]);
        await scope.ServiceProvider.GetRequiredService<IFileStorage>()
            .UploadAsync(file.StorageKey.Value, content, "text/plain", TestContext.Current.CancellationToken);
    }

    private async Task<FileReadDbModel> ReadFileAsync(FileId id)
    {
        await using var scope = Fixture.CreateScope();
        return await scope.ServiceProvider
            .GetRequiredService<FileManagerReadDbContext>()
            .Set<FileReadDbModel>()
            .SingleAsync(file => file.Id == id.Value, TestContext.Current.CancellationToken);
    }

    private sealed class ChangeFileBeforeSaveInterceptor(
        Guid fileId,
        string connectionString,
        bool duringDeletion = false) : SaveChangesInterceptor
    {
        private bool _changed;

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken)
        {
            if (_changed
                || eventData.Context is not FileManagerWriteDbContext context
                || !context.ChangeTracker.Entries<File>().Any(entry =>
                    entry.Entity.Id.Value == fileId
                        && entry.Entity.Status == (duringDeletion ? FileStatus.Deleted : FileStatus.Ready)))
            {
                return result;
            }

            _changed = true;
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var command = new NpgsqlCommand(
                duringDeletion
                    ? "UPDATE file_manager.files SET updated_at = now() WHERE id = @id"
                    : "UPDATE file_manager.files SET name = 'renamed.txt' WHERE id = @id",
                connection);
            command.Parameters.AddWithValue("id", fileId);
            await command.ExecuteNonQueryAsync(cancellationToken);

            return result;
        }
    }
}
