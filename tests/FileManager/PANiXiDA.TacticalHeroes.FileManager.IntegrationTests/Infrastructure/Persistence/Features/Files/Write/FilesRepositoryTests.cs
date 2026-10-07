using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;

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
            file.CompleteUpload(CreateContent());
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
        restored.Purpose.ShouldBe(FilePurpose.Avatar);
        restored.Status.Name.ShouldBe(status);
        restored.Content.ShouldBe(file.Content);
    }

    [Fact(DisplayName = "File repository should persist content completion when upload completes")]
    public async Task UpdateAsync_Should_PersistContent_When_UploadCompletes()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var file = CreateFile();
        var content = CreateContent();
        await SaveNewFileAsync(file, cancellationToken);

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IFilesRepository>();
            var pendingFile = await repository.GetByIdAsync(file.Id, cancellationToken);
            pendingFile!.CompleteUpload(content);
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
        restored.Content.ShouldBe(content);
    }

    [Fact(DisplayName = "File repository should preserve completed deletion and metadata when deletion completes")]
    public async Task UpdateAsync_Should_PersistDeletion_When_DeletionCompletes()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var file = CreateFile();
        file.CompleteUpload(CreateContent());
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
        deleted.Content.ShouldBe(file.Content);
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
        completingFile!.CompleteUpload(CreateContent());

        Func<Task> saveStaleFile = () => completionRepository.UpdateAsync(completingFile, cancellationToken);

        await saveStaleFile.ShouldThrowAsync<DbUpdateConcurrencyException>();
        await using var verificationScope = Fixture.CreateScope();
        var persistedFile = await verificationScope.ServiceProvider
            .GetRequiredService<IFilesRepository>()
            .GetByIdAsync(file.Id, cancellationToken);
        persistedFile.ShouldNotBeNull();
        persistedFile.Status.ShouldBe(FileStatus.Deleting);
        persistedFile.Content.ShouldBeNull();
    }

    private static File CreateFile()
    {
        return File.Create(
            FileName.Create("avatar.png").Value,
            FilePurpose.Avatar);
    }

    private static FileContent CreateContent()
    {
        return FileContent.Create(
            "image/png",
            4096,
            new string('a', 64)).Value;
    }

    private async Task SaveNewFileAsync(File file, CancellationToken cancellationToken)
    {
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IFilesRepository>();

        await repository.AddAsync(file, cancellationToken);
    }
}
