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
    [Fact(DisplayName = "Pending uploads should load tracked aggregates in pages when old personal files exist")]
    public async Task GetPendingPersonalUploadsAsync_Should_LoadTrackedAggregatesInPages_When_OldPersonalFilesExist()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var first = CreatePersonalFile();
        var second = CreatePersonalFile();
        var recent = CreatePersonalFile();
        var atCutoff = CreatePersonalFile();
        foreach (var file in new[] { first, second, recent, atCutoff })
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
            UPDATE file_manager.files SET created_at = {DateTime.UnixEpoch}
            WHERE id IN ({first.Id.Value}, {second.Id.Value})
            """,
            cancellationToken);
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE file_manager.files SET created_at = {cutoff} WHERE id = {atCutoff.Id.Value}",
            cancellationToken);
        var repository = new FilesRepository(
            dbContext: context,
            options: Microsoft.Extensions.Options.Options.Create(new FileUploadReconciliationOptions { BatchSize = 1 }),
            timeProvider: timeProvider);
        var loaded = new List<File>();

        await foreach (var file in repository.GetPendingPersonalUploadsAsync(cancellationToken))
        {
            loaded.Add(file);
        }

        loaded.Select(file => file.Id.Value).ShouldBe(new[] { first.Id.Value, second.Id.Value }.Order());
        loaded[0].Status.ShouldBe(FileStatus.PendingUpload);
        loaded[0].StorageKey.ShouldBe(first.Id == loaded[0].Id ? first.StorageKey : second.StorageKey);
        context.ChangeTracker.Entries<File>().Count().ShouldBe(2);
        context.Entry(loaded[0]).State.ShouldBe(EntityState.Unchanged);
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
        var unitOfWork = scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext));
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
    [InlineData("Deleted")]
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

        if (status is "Deleting" or "Deleted")
        {
            file.BeginDeletion();
        }

        if (status == "Deleted")
        {
            file.CompleteDeletion();
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

    [Fact(DisplayName = "File repository should preserve completed deletion and metadata when deletion completes")]
    public async Task Update_Should_PersistDeletion_When_DeletionCompletes()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var file = CreateFile();
        file.CompleteUpload(
            FileContentType.Create("image/png").Value,
            FileSize.Create(4096).Value);
        await SaveNewFileAsync(file, cancellationToken);

        foreach (var complete in new[] { false, true })
        {
            await using var scope = Fixture.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IFilesRepository>();
            var restored = await repository.GetByIdAsync(file.Id, cancellationToken);
            if (complete)
            {
                restored!.CompleteDeletion();
            }
            else
            {
                restored!.BeginDeletion();
            }

            repository.Update(restored);
            await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext))
                .SaveChangesAsync(cancellationToken);
        }

        await using var verificationScope = Fixture.CreateScope();
        var deleted = await verificationScope.ServiceProvider
            .GetRequiredService<IFilesRepository>()
            .GetByIdAsync(file.Id, cancellationToken);

        deleted.ShouldNotBeNull();
        deleted.Status.ShouldBe(FileStatus.Deleted);
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
