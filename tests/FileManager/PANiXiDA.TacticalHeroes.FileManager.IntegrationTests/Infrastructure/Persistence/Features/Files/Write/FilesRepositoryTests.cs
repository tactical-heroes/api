using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.IntegrationTests.Infrastructure.Persistence.Features.Files.Write;

public sealed class FilesRepositoryTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Theory(DisplayName = "File repository should restore metadata and lifecycle when lifecycle state is saved")]
    [InlineData("PendingUpload")]
    [InlineData("Ready")]
    [InlineData("Deleting")]
    [InlineData("Deleted")]
    public async Task AddAsync_Should_RestoreFile_When_LifecycleStateIsSaved(string status)
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
        restored.Type.ShouldBe(FileType.Avatar);
        restored.FolderId.ShouldBeNull();
        restored.Status.Name.ShouldBe(status);
        restored.ContentType.ShouldBe(file.ContentType);
        restored.Size.ShouldBe(file.Size);
    }

    [Fact(DisplayName = "File repository should persist content completion when upload completes")]
    public async Task UpdateAsync_Should_PersistContent_When_UploadCompletes()
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
            await repository.UpdateAsync(pendingFile, cancellationToken);
        }

        await using var verificationScope = Fixture.CreateScope();
        var restored = await verificationScope.ServiceProvider
            .GetRequiredService<IFilesRepository>()
            .GetByIdAsync(file.Id, cancellationToken);

        restored.ShouldNotBeNull();
        restored.Status.ShouldBe(FileStatus.Ready);
        restored.Name.Value.ShouldBe("new-avatar.png");
        restored.ContentType.ShouldBe(contentType);
        restored.Size.ShouldBe(size);
    }

    [Fact(DisplayName = "File repository should preserve completed deletion and metadata when deletion completes")]
    public async Task UpdateAsync_Should_PersistDeletion_When_DeletionCompletes()
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

            await repository.UpdateAsync(restored, cancellationToken);
        }

        await using var verificationScope = Fixture.CreateScope();
        var deleted = await verificationScope.ServiceProvider
            .GetRequiredService<IFilesRepository>()
            .GetByIdAsync(file.Id, cancellationToken);

        deleted.ShouldNotBeNull();
        deleted.Status.ShouldBe(FileStatus.Deleted);
        deleted.ContentType.ShouldBe(file.ContentType);
        deleted.Size.ShouldBe(file.Size);
    }

    [Fact(DisplayName = "File repository should reject stale completion when deletion was saved first")]
    public async Task UpdateAsync_Should_ThrowConcurrencyException_When_DeletionWasSavedFirst()
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
        await deletionRepository.UpdateAsync(deletingFile, cancellationToken);
        completingFile!.CompleteUpload(
            FileContentType.Create("image/png").Value,
            FileSize.Create(4096).Value);

        Func<Task> saveStaleFile = () => completionRepository.UpdateAsync(completingFile, cancellationToken);

        await saveStaleFile.ShouldThrowAsync<DbUpdateConcurrencyException>();
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
    public async Task UpdateAsync_Should_RestoreFolderPlacement_When_AFileIsAssignedAfterCreation()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var file = CreateFile();
        await SaveNewFileAsync(file, cancellationToken);
        var folder = Folder.Create(
            name: FolderName.Create("Avatars").Value,
            allowedFileType: FileType.Avatar).Value;
        await using (var scope = Fixture.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<IFoldersRepository>()
                .AddAsync(folder, cancellationToken);
            var repository = scope.ServiceProvider.GetRequiredService<IFilesRepository>();
            var stored = await repository.GetByIdAsync(file.Id, cancellationToken);
            stored!.MoveTo(folder).IsSuccess.ShouldBeTrue();

            await repository.UpdateAsync(stored, cancellationToken);
        }

        await using var verificationScope = Fixture.CreateScope();
        var restored = await verificationScope.ServiceProvider.GetRequiredService<IFilesRepository>()
            .GetByIdAsync(file.Id, cancellationToken);
        restored.ShouldNotBeNull();
        restored.FolderId.ShouldBe(folder.Id);
        restored.Type.ShouldBe(FileType.Avatar);
        restored.Status.ShouldBe(FileStatus.PendingUpload);
    }

    [Fact(DisplayName = "File repository should reject a dangling folder reference when folder does not exist")]
    public async Task AddAsync_Should_RejectDanglingFolderReference_When_FolderDoesNotExist()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var file = CreateFile();
        var folder = Folder.Create(
            name: FolderName.Create("Avatars").Value,
            allowedFileType: FileType.Avatar).Value;
        file.MoveTo(folder).IsSuccess.ShouldBeTrue();

        Func<Task> saveFile = () => SaveNewFileAsync(file, cancellationToken);

        var exception = await saveFile.ShouldThrowAsync<DbUpdateException>();
        exception.InnerException.ShouldBeOfType<PostgresException>()
            .SqlState.ShouldBe(PostgresErrorCodes.ForeignKeyViolation);
        await using var scope = Fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
        (await context.Set<File>().AnyAsync(cancellationToken)).ShouldBeFalse();
    }

    private static File CreateFile()
    {
        return File.Create(
            FileName.Create("avatar.png").Value,
            FileType.Avatar).Value;
    }

    private async Task SaveNewFileAsync(File file, CancellationToken cancellationToken)
    {
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IFilesRepository>();

        await repository.AddAsync(file, cancellationToken);
    }
}
