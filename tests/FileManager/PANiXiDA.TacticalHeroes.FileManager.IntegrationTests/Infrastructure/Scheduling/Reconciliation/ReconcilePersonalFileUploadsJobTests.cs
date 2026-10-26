using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

using PANiXiDA.Core.Application.Storage;

using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Read.DbModels;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Scheduling.Options.FileUploadReconciliation;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Scheduling.Reconciliation;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Storage;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.IntegrationTests.Infrastructure.Scheduling.Reconciliation;

public sealed class ReconcilePersonalFileUploadsJobTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact(DisplayName = "Reconciliation should recover ready metadata when uploaded object exists")]
    public async Task ExecuteAsync_Should_RecoverReadyMetadata_When_UploadedObjectExists()
    {
        var file = await SaveFileAsync();
        await UploadAsync(file);
        await using var scope = Fixture.CreateScope();

        await CreateJob(scope.ServiceProvider).ExecuteAsync(TestContext.Current.CancellationToken);

        var stored = await ReadFileAsync(file.Id);
        stored.Status.ShouldBe("Ready");
        stored.Size.ShouldBe(4);
        stored.ContentType.ShouldBe("text/plain");
        stored.StorageKey.ShouldBe(file.StorageKey.Value);
    }

    [Fact(DisplayName = "Reconciliation should retry pending content when object arrives after an earlier scan")]
    public async Task ExecuteAsync_Should_RetryPendingContent_When_ObjectArrivesAfterAnEarlierScan()
    {
        var file = await SaveFileAsync();
        await using var scope = Fixture.CreateScope();
        var job = CreateJob(scope.ServiceProvider);
        await job.ExecuteAsync(TestContext.Current.CancellationToken);
        (await ReadFileAsync(file.Id)).Status.ShouldBe("PendingUpload");

        await UploadAsync(file);
        await job.ExecuteAsync(TestContext.Current.CancellationToken);

        (await ReadFileAsync(file.Id)).Status.ShouldBe("Ready");
    }

    [Theory(DisplayName = "Reconciliation should leave unrelated records unchanged when file is ineligible")]
    [InlineData("recent")]
    [InlineData("avatar")]
    [InlineData("ready")]
    [InlineData("deleting")]
    [InlineData("deleted")]
    [InlineData("disabled")]
    public async Task ExecuteAsync_Should_LeaveUnrelatedRecordsUnchanged_When_FileIsIneligible(string condition)
    {
        var file = await SaveFileAsync(condition);
        await UploadAsync(file);
        var original = await ReadFileAsync(file.Id);
        await using var scope = Fixture.CreateScope();

        await CreateJob(scope.ServiceProvider, enabled: condition != "disabled")
            .ExecuteAsync(TestContext.Current.CancellationToken);

        var stored = await ReadFileAsync(file.Id);
        stored.Status.ShouldBe(original.Status);
        stored.Size.ShouldBe(original.Size);
        stored.ContentType.ShouldBe(original.ContentType);
    }

    [Fact(DisplayName = "Reconciliation should visit every page when earlier files are still missing")]
    public async Task ExecuteAsync_Should_VisitEveryPage_When_EarlierFilesAreStillMissing()
    {
        var files = new[] { await SaveFileAsync(), await SaveFileAsync(), await SaveFileAsync() };
        var ordered = files.OrderBy(file => file.Id.Value).ToArray();
        await UploadAsync(ordered[1]);
        await UploadAsync(ordered[2]);
        await using var scope = Fixture.CreateScope();

        await CreateJob(scope.ServiceProvider, batchSize: 1).ExecuteAsync(TestContext.Current.CancellationToken);

        (await ReadFileAsync(ordered[0].Id)).Status.ShouldBe("PendingUpload");
        (await ReadFileAsync(ordered[1].Id)).Status.ShouldBe("Ready");
        (await ReadFileAsync(ordered[2].Id)).Status.ShouldBe("Ready");
    }

    [Fact(DisplayName = "Reconciliation should preserve pending state when storage is unavailable")]
    public async Task ExecuteAsync_Should_PreservePendingState_When_StorageIsUnavailable()
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

            await CreateJob(scope.ServiceProvider).ExecuteAsync(TestContext.Current.CancellationToken);

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
    public async Task ExecuteAsync_Should_PropagateCancellation_When_CancellationIsRequested()
    {
        await using var scope = Fixture.CreateScope();
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        await cancellation.CancelAsync();

        var exception = await Record.ExceptionAsync(() =>
            CreateJob(scope.ServiceProvider).ExecuteAsync(cancellation.Token));

        exception.ShouldBeAssignableTo<OperationCanceledException>();
    }

    [Theory(DisplayName = "Reconciliation should preserve pending state when stored metadata is invalid")]
    [InlineData(0, "text/plain")]
    [InlineData(4, "text/*")]
    public async Task ExecuteAsync_Should_KeepPending_When_StoredMetadataIsInvalid(int size, string contentType)
    {
        var file = await SaveFileAsync();
        await using var scope = Fixture.CreateScope();
        await using var content = new MemoryStream(new byte[size]);
        await scope.ServiceProvider.GetRequiredService<IFileStorage>()
            .UploadAsync(file.StorageKey.Value, content, contentType, TestContext.Current.CancellationToken);

        await CreateJob(scope.ServiceProvider).ExecuteAsync(TestContext.Current.CancellationToken);

        var stored = await ReadFileAsync(file.Id);
        stored.Status.ShouldBe("PendingUpload");
        stored.Size.ShouldBeNull();
        stored.ContentType.ShouldBeNull();
    }

    private static ReconcilePersonalFileUploadsJob CreateJob(
        IServiceProvider services,
        bool enabled = true,
        int batchSize = 100)
    {
        return new ReconcilePersonalFileUploadsJob(
            services.GetRequiredService<FileManagerReadDbContext>(),
            services.GetRequiredService<S3FileMetadataReader>(),
            services.GetRequiredService<IServiceScopeFactory>(),
            TimeProvider.System,
            Microsoft.Extensions.Options.Options.Create(new FileUploadReconciliationOptions
            {
                Enabled = enabled,
                BatchSize = batchSize
            }),
            NullLogger<ReconcilePersonalFileUploadsJob>.Instance);
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
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE file_manager.files SET created_at = {DateTime.UnixEpoch} WHERE id = {file.Id.Value}",
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
}
