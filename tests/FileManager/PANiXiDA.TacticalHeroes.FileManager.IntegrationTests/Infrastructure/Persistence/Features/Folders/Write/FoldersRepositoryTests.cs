using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.FileManager.IntegrationTests.Infrastructure.Persistence.Features.Folders.Write;

public sealed class FoldersRepositoryTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Theory(DisplayName = "Folder repository should restore a folder when depth varies")]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    public async Task AddAsync_Should_RestoreFolder_When_DepthVaries(int depth)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var folder = Folder.Create(
            name: FolderName.Create("Avatars").Value,
            allowedFileType: FileType.Avatar);
        await SaveNewFolderAsync(folder, cancellationToken);
        for (var level = 0; level < depth; level++)
        {
            folder = folder.CreateChild(FolderName.Create($"Level {level}").Value);
            await SaveNewFolderAsync(folder, cancellationToken);
        }

        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IFoldersRepository>();

        var restored = await repository.GetByIdAsync(folder.Id, cancellationToken);

        restored.ShouldNotBeNull();
        restored.Id.ShouldBe(folder.Id);
        restored.Name.ShouldBe(folder.Name);
        restored.AllowedFileType.ShouldBe(FileType.Avatar);
        restored.ParentId.ShouldBe(folder.ParentId);
        scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>()
            .ChangeTracker.Entries().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Folder repository should preserve placement when folder is renamed")]
    public async Task UpdateAsync_Should_PersistNameAndPreservePlacement_When_FolderIsRenamed()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var parent = Folder.Create(
            name: FolderName.Create("Avatars").Value,
            allowedFileType: FileType.Avatar);
        var folder = parent.CreateChild(FolderName.Create("Players").Value);
        await SaveNewFolderAsync(parent, cancellationToken);
        await SaveNewFolderAsync(folder, cancellationToken);

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IFoldersRepository>();
            var restored = await repository.GetByIdAsync(folder.Id, cancellationToken);
            restored!.Rename(FolderName.Create("Heroes").Value);

            await repository.UpdateAsync(restored, cancellationToken);
        }

        await using var verificationScope = Fixture.CreateScope();
        var saved = await verificationScope.ServiceProvider
            .GetRequiredService<IFoldersRepository>()
            .GetByIdAsync(folder.Id, cancellationToken);
        saved.ShouldNotBeNull();
        saved.Name.Value.ShouldBe("Heroes");
        saved.AllowedFileType.ShouldBe(FileType.Avatar);
        saved.ParentId.ShouldBe(parent.Id);
    }

    [Fact(DisplayName = "Folder repository should reject an orphan when parent does not exist")]
    public async Task AddAsync_Should_RejectOrphan_When_ParentDoesNotExist()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var parent = Folder.Create(
            name: FolderName.Create("Avatars").Value,
            allowedFileType: FileType.Avatar);
        var child = parent.CreateChild(FolderName.Create("Players").Value);

        Func<Task> saveOrphan = () => SaveNewFolderAsync(child, cancellationToken);

        var exception = await saveOrphan.ShouldThrowAsync<DbUpdateException>();
        exception.InnerException.ShouldBeOfType<PostgresException>()
            .SqlState.ShouldBe(PostgresErrorCodes.ForeignKeyViolation);
        await using var scope = Fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
        (await context.Set<Folder>().AnyAsync(cancellationToken)).ShouldBeFalse();
    }

    [Fact(DisplayName = "Folder foreign key should prevent cascading deletion when children exist")]
    public async Task ExecuteDeleteAsync_Should_RejectParentDeletion_When_ChildrenExist()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var parent = Folder.Create(
            name: FolderName.Create("Avatars").Value,
            allowedFileType: FileType.Avatar);
        var child = parent.CreateChild(FolderName.Create("Players").Value);
        await SaveNewFolderAsync(parent, cancellationToken);
        await SaveNewFolderAsync(child, cancellationToken);
        await using var scope = Fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();

        Func<Task> deleteParent = () => context.Set<Folder>()
            .Where(folder => folder.Id == parent.Id)
            .ExecuteDeleteAsync(cancellationToken);

        var exception = await deleteParent.ShouldThrowAsync<PostgresException>();
        exception.SqlState.ShouldBe(PostgresErrorCodes.ForeignKeyViolation);
        (await context.Set<Folder>().CountAsync(cancellationToken)).ShouldBe(2);
    }

    private async Task SaveNewFolderAsync(Folder folder, CancellationToken cancellationToken)
    {
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IFoldersRepository>();

        await repository.AddAsync(folder, cancellationToken);
    }
}
