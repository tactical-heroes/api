using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using NSubstitute;

using PANiXiDA.Core.Application.Persistence.Errors;

using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Write;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Scheduling.Options.FileUploadReconciliation;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.IntegrationTests.Infrastructure.Persistence.Features.Files.Write;

public sealed class FilesRepositoryTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact(DisplayName = "Pending uploads should load tracked aggregates in pages when old files exist")]
    public async Task GetPendingUploadsAsync_Should_LoadTrackedAggregatesInPages_When_OldFilesExist()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var first = CreatePersonalFile();
        var second = CreatePersonalFile();
        var recent = CreatePersonalFile();
        var atCutoff = CreatePersonalFile();
        var expired = CreatePersonalFile();
        var avatar = CreateFile();
        foreach (var file in new[] { first, second, recent, atCutoff, expired, avatar })
        {
            await SaveNewFileAsync(file, cancellationToken);
        }

        var now = DateTimeOffset.UtcNow;
        var cutoff = now.AddMinutes(-15).UtcDateTime;
        var timeProvider = Substitute.For<TimeProvider>();
        timeProvider.GetUtcNow().Returns(now, now.AddHours(1));
        await using var scope = Fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"""
            UPDATE file_manager.files SET created_at = {now.AddHours(-1).UtcDateTime}
            WHERE id IN ({first.Id.Value}, {second.Id.Value}, {avatar.Id.Value})
            """,
            cancellationToken);
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE file_manager.files SET created_at = {DateTime.UnixEpoch} WHERE id = {expired.Id.Value}",
            cancellationToken);
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE file_manager.files SET created_at = {cutoff} WHERE id = {atCutoff.Id.Value}",
            cancellationToken);
        var repository = new FilesRepository(
            dbContext: context,
            options: Microsoft.Extensions.Options.Options.Create(new FileUploadReconciliationOptions { BatchSize = 1 }),
            timeProvider: timeProvider);
        var loaded = new List<File>();

        await foreach (var file in repository.GetPendingUploadsAsync(cancellationToken))
        {
            loaded.Add(file);
        }

        var expectedIds = new[] { first.Id.Value, second.Id.Value, avatar.Id.Value }.Order();
        loaded.Select(file => file.Id.Value).ShouldBe(expectedIds);
        loaded[0].Status.ShouldBe(FileStatus.PendingUpload);
        var firstExpected = new[] { first, second, avatar }.Single(file => file.Id == loaded[0].Id);
        loaded[0].StorageKey.ShouldBe(firstExpected.StorageKey);
        context.ChangeTracker.Entries<File>().Count().ShouldBe(3);
        context.Entry(loaded[0]).State.ShouldBe(EntityState.Unchanged);
        context.Entry(loaded[0]).Property<uint>("xmin").OriginalValue.ShouldBeGreaterThan(0u);
        timeProvider.Received(1).GetUtcNow();
    }

    [Fact(DisplayName = "Expired uploads should use a fixed cutoff when tracked aggregates are paged")]
    public async Task GetExpiredUploadsAsync_Should_UseFixedCutoff_When_TrackedAggregatesArePaged()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var first = CreatePersonalFile();
        var second = CreatePersonalFile();
        var atCutoff = CreatePersonalFile();
        var recent = CreatePersonalFile();
        var avatar = CreateFile();
        foreach (var file in new[] { first, second, atCutoff, recent, avatar })
        {
            await SaveNewFileAsync(file, cancellationToken);
        }

        var now = DateTimeOffset.UtcNow;
        var timeProvider = Substitute.For<TimeProvider>();
        timeProvider.GetUtcNow().Returns(now, now.AddHours(1));
        await using var scope = Fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"""
            UPDATE file_manager.files SET created_at = {DateTime.UnixEpoch}
            WHERE id IN ({first.Id.Value}, {second.Id.Value}, {avatar.Id.Value})
            """,
            cancellationToken);
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE file_manager.files SET created_at = {now.AddDays(-1).UtcDateTime} WHERE id = {atCutoff.Id.Value}",
            cancellationToken);
        var repository = new FilesRepository(
            dbContext: context,
            options: Microsoft.Extensions.Options.Options.Create(new FileUploadReconciliationOptions { BatchSize = 1 }),
            timeProvider: timeProvider);
        var loaded = new List<File>();

        await foreach (var file in repository.GetExpiredUploadsAsync(cancellationToken))
        {
            loaded.Add(file);
        }

        var expectedIds = new[] { first.Id.Value, second.Id.Value, avatar.Id.Value }.Order();
        loaded.Select(file => file.Id.Value).ShouldBe(expectedIds);
        context.ChangeTracker.Entries<File>().Count().ShouldBe(3);
        context.Entry(loaded[0]).State.ShouldBe(EntityState.Unchanged);
        context.Entry(loaded[0]).Property<uint>("xmin").OriginalValue.ShouldBeGreaterThan(0u);
        timeProvider.Received(1).GetUtcNow();
    }

    [Fact(DisplayName = "File deletion queries should separate active and deleted files when lifecycle states vary")]
    public async Task GetFilesForDeletionAsync_Should_SeparateActiveAndDeletedFiles_When_LifecycleStatesVary()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var deleting = CreatePersonalFile();
        var deleted = CreatePersonalFile();
        var deletedReady = CreatePersonalFile();
        var pending = CreatePersonalFile();
        var avatar = CreateFile();
        var activeAvatar = CreateFile();
        deleting.BeginDeletion();
        deleted.BeginDeletion();
        deletedReady.CompleteUpload(FileContentType.Create("text/plain").Value, FileSize.Create(4).Value);
        avatar.BeginDeletion();
        activeAvatar.BeginDeletion();
        foreach (var file in new[] { deleting, deleted, deletedReady, pending, avatar, activeAvatar })
        {
            await SaveNewFileAsync(file, cancellationToken);
        }

        await using (var deletionScope = Fixture.CreateScope())
        {
            var deletionRepository = deletionScope.ServiceProvider.GetRequiredService<IFilesRepository>();
            foreach (var file in new[] { deleted, deletedReady, avatar })
            {
                var stored = await deletionRepository.GetByIdAsync(file.Id, cancellationToken);
                deletionRepository.Delete(stored!);
            }

            await deletionScope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext))
                .SaveChangesAsync(cancellationToken);
        }

        await using var scope = Fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
        var repository = new FilesRepository(
            dbContext: context,
            options: Microsoft.Extensions.Options.Options.Create(new FileUploadReconciliationOptions { BatchSize = 1 }),
            timeProvider: TimeProvider.System);
        var active = new List<File>();
        var tombstones = new List<File>();

        await foreach (var file in repository.GetFilesForDeletionAsync(cancellationToken))
        {
            active.Add(file);
        }

        await foreach (var file in repository.GetDeletedFilesAsync(cancellationToken))
        {
            tombstones.Add(file);
        }

        active.Select(file => file.Id.Value).ShouldBe(new[] { deleting.Id.Value, activeAvatar.Id.Value }.Order());
        var expectedTombstones = new[] { deleted.Id.Value, deletedReady.Id.Value, avatar.Id.Value }.Order();
        tombstones.Select(file => file.Id.Value).ShouldBe(expectedTombstones);
        context.Entry(tombstones[0]).Property<DateTime?>(EfConstants.DeletedAt).CurrentValue.ShouldNotBeNull();
        context.Entry(tombstones[0]).Property<uint>("xmin").OriginalValue.ShouldBeGreaterThan(0u);
        (await repository.GetByIdAsync(deleted.Id, cancellationToken)).ShouldBeNull();
    }

    [Fact(DisplayName = "Deleted files should use a fixed recheck window when aggregates are paged")]
    public async Task GetDeletedFilesAsync_Should_UseFixedRecheckWindow_When_AggregatesArePaged()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var recent = CreatePersonalFile();
        var atCutoff = CreateFile();
        var expired = CreatePersonalFile();
        var atStart = CreateFile();
        var future = CreatePersonalFile();
        var active = CreatePersonalFile();
        foreach (var file in new[] { recent, atCutoff, expired, atStart, future, active })
        {
            await SaveNewFileAsync(file, cancellationToken);
        }

        var now = DateTimeOffset.UtcNow;
        now = now.AddTicks(-(now.Ticks % 10));
        var window = TimeSpan.FromHours(2);
        var cutoff = now.Subtract(window).UtcDateTime;
        var timeProvider = Substitute.For<TimeProvider>();
        timeProvider.GetUtcNow().Returns(now, now.AddHours(3));
        await using var scope = Fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
        var deletionTimes = new[]
        {
            (recent, now.AddHours(-1).UtcDateTime),
            (atCutoff, cutoff),
            (expired, cutoff.AddTicks(-10)),
            (atStart, now.UtcDateTime),
            (future, now.AddTicks(10).UtcDateTime)
        };
        foreach (var (file, deletedAt) in deletionTimes)
        {
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE file_manager.files SET deleted_at = {deletedAt} WHERE id = {file.Id.Value}",
                cancellationToken);
        }

        var repository = new FilesRepository(
            dbContext: context,
            options: Microsoft.Extensions.Options.Options.Create(new FileUploadReconciliationOptions
            {
                BatchSize = 1,
                DeletedFileRecheckWindow = window
            }),
            timeProvider: timeProvider);
        var loaded = new List<File>();

        await foreach (var file in repository.GetDeletedFilesAsync(cancellationToken))
        {
            loaded.Add(file);
        }

        var expectedIds = new[] { recent.Id.Value, atCutoff.Id.Value, atStart.Id.Value }.Order();
        loaded.Select(file => file.Id.Value).ShouldBe(expectedIds);
        context.ChangeTracker.Entries<File>().Count().ShouldBe(3);
        context.Entry(loaded[0]).Property<uint>("xmin").OriginalValue.ShouldBeGreaterThan(0u);
        timeProvider.Received(1).GetUtcNow();
    }

    [Fact(DisplayName = "Add should stage a tracked file when unit of work has not saved")]
    public async Task Add_Should_StageTrackedFile_When_UnitOfWorkHasNotSaved()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var scope = Fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IFilesRepository>();
        var tracker = scope.ServiceProvider.GetRequiredService<IAggregateTracker>();
        var unitOfWork = scope.ServiceProvider
            .GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext));
        var file = CreateFile();

        repository.Add(file);

        context.Entry(file).State.ShouldBe(EntityState.Added);
        tracker.GetAll().ShouldContain(file);
        (await context.Set<File>().AnyAsync(cancellationToken)).ShouldBeFalse();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        context.Entry(file).State.ShouldBe(EntityState.Unchanged);
        context.Entry(file).Property<uint>("xmin").OriginalValue.ShouldBeGreaterThan(0u);
        (await context.Set<File>().AnyAsync(cancellationToken)).ShouldBeTrue();
    }

    [Fact(DisplayName = "File repository should restore the persisted key when key uses an older format")]
    public async Task GetByIdAsync_Should_RestorePersistedKey_When_KeyUsesAnOlderFormat()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var file = CreateFile();
        await SaveNewFileAsync(file, cancellationToken);
        const string StoredKey = "legacy/Original-File.png";
        await using (var scope = Fixture.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE file_manager.files SET storage_key = {StoredKey} WHERE id = {file.Id.Value}",
                cancellationToken);
        }

        await using var verificationScope = Fixture.CreateScope();
        var restored = await verificationScope.ServiceProvider.GetRequiredService<IFilesRepository>()
            .GetByIdAsync(file.Id, cancellationToken);

        restored.ShouldNotBeNull();
        restored.StorageKey.Value.ShouldBe(StoredKey);
    }

    [Theory(DisplayName = "File repository should restore metadata and lifecycle when lifecycle state is saved")]
    [InlineData("PendingUpload")]
    [InlineData("Ready")]
    [InlineData("Deleting")]
    public async Task Add_Should_RestoreFile_When_LifecycleStateIsSaved(string status)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var file = CreateFile();
        if (status != "PendingUpload")
        {
            file.CompleteUpload(
                FileContentType.Create("image/png").Value,
                FileSize.Create(4096).Value);
        }

        if (status == "Deleting")
        {
            file.BeginDeletion();
        }

        await SaveNewFileAsync(file, cancellationToken);
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IFilesRepository>();
        var restored = await repository.GetByIdAsync(file.Id, cancellationToken);

        restored.ShouldNotBeNull();
        restored.Id.ShouldBe(file.Id);
        restored.Name.ShouldBe(file.Name);
        restored.StorageKey.ShouldBe(file.StorageKey);
        restored.Type.ShouldBe(FileType.Avatar);
        restored.FolderId.ShouldBeNull();
        restored.Status.Name.ShouldBe(status);
        restored.ContentType.ShouldBe(file.ContentType);
        restored.Size.ShouldBe(file.Size);
    }

    [Fact(DisplayName = "File repository should persist content completion when upload completes")]
    public async Task Update_Should_PersistContent_When_UploadCompletes()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var file = CreateFile();
        var contentType = FileContentType.Create("image/png").Value;
        var size = FileSize.Create(4096).Value;
        await SaveNewFileAsync(file, cancellationToken);

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IFilesRepository>();
            var pendingFile = await repository.GetByIdAsync(file.Id, cancellationToken);
            pendingFile!.CompleteUpload(
                contentType,
                size);
            pendingFile.Rename(FileName.Create("new-avatar.png").Value);
            repository.Update(pendingFile);
            await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext))
                .SaveChangesAsync(cancellationToken);
        }

        await using var verificationScope = Fixture.CreateScope();
        var restored = await verificationScope.ServiceProvider
            .GetRequiredService<IFilesRepository>()
            .GetByIdAsync(file.Id, cancellationToken);

        restored.ShouldNotBeNull();
        restored.Status.ShouldBe(FileStatus.Ready);
        restored.Name.Value.ShouldBe("new-avatar.png");
        restored.StorageKey.ShouldBe(file.StorageKey);
        restored.ContentType.ShouldBe(contentType);
        restored.Size.ShouldBe(size);
    }

    [Fact(DisplayName = "File repository should set DeletedAt and preserve metadata when deletion completes")]
    public async Task Delete_Should_SetDeletedAtAndPreserveMetadata_When_DeletionCompletes()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var file = CreateFile();
        file.CompleteUpload(FileContentType.Create("image/png").Value, FileSize.Create(4096).Value);
        await SaveNewFileAsync(file, cancellationToken);
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IFilesRepository>();
        var context = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
        var unitOfWork = scope.ServiceProvider
            .GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext));
        var stored = await repository.GetByIdAsync(file.Id, cancellationToken);
        stored!.BeginDeletion();
        repository.Update(stored);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        repository.Delete(stored);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        (await repository.GetByIdAsync(file.Id, cancellationToken)).ShouldBeNull();
        var deleted = await context.Set<File>().IgnoreQueryFilters().SingleAsync(
            item => item.Id == file.Id, cancellationToken);
        context.Entry(deleted).Property<DateTime?>(EfConstants.DeletedAt).CurrentValue.ShouldNotBeNull();
        deleted.Status.ShouldBe(FileStatus.Deleting);
        deleted.StorageKey.ShouldBe(file.StorageKey);
        deleted.ContentType.ShouldBe(file.ContentType);
        deleted.Size.ShouldBe(file.Size);
    }

    [Fact(DisplayName = "File repository should reject stale completion when deletion was saved first")]
    public async Task SaveChangesAsync_Should_ThrowConcurrencyException_When_DeletionWasSavedFirst()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var file = CreateFile();
        await SaveNewFileAsync(file, cancellationToken);
        await using var deletionScope = Fixture.CreateScope();
        await using var completionScope = Fixture.CreateScope();
        var deletionRepository = deletionScope.ServiceProvider.GetRequiredService<IFilesRepository>();
        var completionRepository = completionScope.ServiceProvider.GetRequiredService<IFilesRepository>();
        var deletingFile = await deletionRepository.GetByIdAsync(file.Id, cancellationToken);
        var completingFile = await completionRepository.GetByIdAsync(file.Id, cancellationToken);
        deletingFile!.BeginDeletion();
        deletionRepository.Update(deletingFile);
        await deletionScope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext))
            .SaveChangesAsync(cancellationToken);
        completingFile!.CompleteUpload(
            FileContentType.Create("image/png").Value,
            FileSize.Create(4096).Value);

        completionRepository.Update(completingFile);
        Func<Task> saveStaleFile = () => completionScope.ServiceProvider
            .GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext))
            .SaveChangesAsync(cancellationToken);

        var exception = await saveStaleFile.ShouldThrowAsync<ConcurrencyException>();
        exception.InnerException.ShouldBeOfType<DbUpdateConcurrencyException>();
        await using var verificationScope = Fixture.CreateScope();
        var persistedFile = await verificationScope.ServiceProvider
            .GetRequiredService<IFilesRepository>()
            .GetByIdAsync(file.Id, cancellationToken);
        persistedFile.ShouldNotBeNull();
        persistedFile.Status.ShouldBe(FileStatus.Deleting);
        persistedFile.ContentType.ShouldBeNull();
        persistedFile.Size.ShouldBeNull();
    }

    [Fact(DisplayName = "File repository should restore folder placement when a file is assigned after creation")]
    public async Task Update_Should_RestoreFolderPlacement_When_AFileIsAssignedAfterCreation()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var file = CreateFile();
        await SaveNewFileAsync(file, cancellationToken);
        var folder = Folder.Create(
            name: FolderName.Create("Avatars").Value,
            allowedFileType: FileType.Avatar,
            userId: null).Value;
        await using (var scope = Fixture.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<IFoldersRepository>().Add(folder);
            await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext))
                .SaveChangesAsync(cancellationToken);
            var repository = scope.ServiceProvider.GetRequiredService<IFilesRepository>();
            var stored = await repository.GetByIdAsync(file.Id, cancellationToken);
            stored!.MoveTo(folder).IsSuccess.ShouldBeTrue();

            repository.Update(stored);
            await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext))
                .SaveChangesAsync(cancellationToken);
        }

        await using var verificationScope = Fixture.CreateScope();
        var restored = await verificationScope.ServiceProvider.GetRequiredService<IFilesRepository>()
            .GetByIdAsync(file.Id, cancellationToken);
        restored.ShouldNotBeNull();
        restored.FolderId.ShouldBe(folder.Id);
        restored.StorageKey.ShouldBe(file.StorageKey);
        restored.Type.ShouldBe(FileType.Avatar);
        restored.Status.ShouldBe(FileStatus.PendingUpload);
    }

    [Fact(DisplayName = "File repository should reject a dangling folder reference when folder does not exist")]
    public async Task Add_Should_RejectDanglingFolderReference_When_FolderDoesNotExist()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var file = CreateFile();
        var folder = Folder.Create(
            name: FolderName.Create("Avatars").Value,
            allowedFileType: FileType.Avatar,
            userId: null).Value;
        file.MoveTo(folder).IsSuccess.ShouldBeTrue();

        Func<Task> saveFile = () => SaveNewFileAsync(file, cancellationToken);

        var exception = await saveFile.ShouldThrowAsync<DbUpdateException>();
        exception.InnerException.ShouldBeOfType<PostgresException>()
            .SqlState
            .ShouldBe(PostgresErrorCodes.ForeignKeyViolation);
        await using var scope = Fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
        (await context.Set<File>().AnyAsync(cancellationToken)).ShouldBeFalse();
    }

    [Fact(DisplayName = "Folder file selection should skip processed entries when parent is soft deleted")]
    public async Task GetBatchByFolderForDeletionAsync_Should_SkipProcessedEntries_When_ParentIsSoftDeleted()
    {
        var ct = TestContext.Current.CancellationToken;
        var root = Folder.Create(FolderName.Create("Root").Value, FileType.Avatar, userId: null).Value;
        var nested = root.CreateChild(FolderName.Create("Nested").Value);
        var files = Enumerable.Range(0, 106).Select(_ => CreateFile()).ToList();
        foreach (var file in files)
        {
            file.MoveTo(root);
        }
        files[4].MoveTo(nested);
        files[1].CompleteUpload(FileContentType.Create("image/png").Value, FileSize.Create(1).Value);
        files[2].BeginDeletion();
        await using (var scope = Fixture.CreateScope())
        {
            var folders = scope.ServiceProvider.GetRequiredService<IFoldersRepository>();
            folders.Add(root);
            folders.Add(nested);
            var repository = scope.ServiceProvider.GetRequiredService<IFilesRepository>();
            foreach (var file in files)
            {
                repository.Add(file);
            }
            var unitOfWork = scope.ServiceProvider
                .GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext));
            await unitOfWork.SaveChangesAsync(ct);
            repository.Delete(files[3]);
            await unitOfWork.SaveChangesAsync(ct);
        }
        await using (var scope = Fixture.CreateScope())
        {
            var folders = scope.ServiceProvider.GetRequiredService<IFoldersRepository>();
            var stored = await folders.GetByIdAsync(root.Id, ct);
            folders.Delete(stored!);
            await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext))
                .SaveChangesAsync(ct);
        }
        await using var check = Fixture.CreateScope();
        var repo = check.ServiceProvider.GetRequiredService<IFilesRepository>();

        var loaded = await repo.GetBatchByFolderForDeletionAsync(root.Id, ct);

        var expectedIds = files.Where((_, index) => index is not (2 or 3 or 4))
            .Select(file => file.Id.Value)
            .Order()
            .Take(100);
        loaded.Select(file => file.Id.Value).ShouldBe(expectedIds);
        var db = check.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
        db.ChangeTracker.Entries<File>().Count().ShouldBe(100);
        db.ChangeTracker.Entries<Folder>().ShouldBeEmpty();
    }

    private static File CreateFile()
    {
        return File.Create(
            FileName.Create("avatar.png").Value,
            FileType.Avatar,
            userId: null).Value;
    }

    private static File CreatePersonalFile()
    {
        return File.Create(
            name: FileName.Create("file.txt").Value,
            type: FileType.Personal,
            userId: UserId.Create(Guid.NewGuid()).Value).Value;
    }

    private async Task SaveNewFileAsync(File file, CancellationToken cancellationToken)
    {
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IFilesRepository>();

        repository.Add(file);
        await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext))
            .SaveChangesAsync(cancellationToken);
    }
}
