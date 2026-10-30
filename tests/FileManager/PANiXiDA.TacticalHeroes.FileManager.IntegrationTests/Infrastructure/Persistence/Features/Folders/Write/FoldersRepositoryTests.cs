using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using PANiXiDA.Core.Application.Persistence.Errors;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.IntegrationTests.Infrastructure.Persistence.Features.Folders.Write;

public sealed class FoldersRepositoryTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Theory(DisplayName = "Folder repository should restore a folder when depth and ownership vary")]
    [InlineData(0, false)]
    [InlineData(1, false)]
    [InlineData(3, false)]
    [InlineData(0, true)]
    [InlineData(1, true)]
    [InlineData(3, true)]
    public async Task Add_Should_RestoreFolder_When_DepthAndOwnershipVary(int depth, bool personal)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        UserId? userId = personal ? UserId.Create(Guid.CreateVersion7()).Value : null;
        var folder = Folder.Create(
            name: FolderName.Create("Avatars").Value,
            allowedFileType: personal ? FileType.Personal : FileType.Avatar,
            userId: userId).Value;
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
        restored.AllowedFileType.ShouldBe(personal ? FileType.Personal : FileType.Avatar);
        restored.UserId.ShouldBe(userId);
        restored.ParentId.ShouldBe(folder.ParentId);
        scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>()
            .ChangeTracker
            .Entries<Folder>()
            .ShouldHaveSingleItem()
            .Entity
            .ShouldBeSameAs(restored);
    }

    [Fact(DisplayName = "Folder repository should preserve placement when folder is renamed")]
    public async Task Update_Should_PersistNameAndPreservePlacement_When_FolderIsRenamed()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var parent = Folder.Create(
            name: FolderName.Create("Avatars").Value,
            allowedFileType: FileType.Avatar,
            userId: null).Value;
        var folder = parent.CreateChild(FolderName.Create("Players").Value);
        await SaveNewFolderAsync(parent, cancellationToken);
        await SaveNewFolderAsync(folder, cancellationToken);

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IFoldersRepository>();
            var restored = await repository.GetByIdAsync(folder.Id, cancellationToken);
            restored!.Rename(FolderName.Create("Heroes").Value);

            repository.Update(restored);
            await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext))
                .SaveChangesAsync(cancellationToken);
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

    [Fact(DisplayName = "Folder repository should reject a stale rename when another rename was saved first")]
    public async Task SaveChangesAsync_Should_ThrowConcurrencyException_When_AnotherRenameWasSavedFirst()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var folder = Folder.Create(
            name: FolderName.Create("Avatars").Value,
            allowedFileType: FileType.Avatar,
            userId: null).Value;
        await SaveNewFolderAsync(folder, cancellationToken);
        await using var firstScope = Fixture.CreateScope();
        await using var secondScope = Fixture.CreateScope();
        var firstRepository = firstScope.ServiceProvider.GetRequiredService<IFoldersRepository>();
        var secondRepository = secondScope.ServiceProvider.GetRequiredService<IFoldersRepository>();
        var firstFolder = await firstRepository.GetByIdAsync(folder.Id, cancellationToken);
        var secondFolder = await secondRepository.GetByIdAsync(folder.Id, cancellationToken);
        firstFolder!.Rename(FolderName.Create("Players").Value);
        firstRepository.Update(firstFolder);
        await firstScope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext))
            .SaveChangesAsync(cancellationToken);
        secondFolder!.Rename(FolderName.Create("Heroes").Value);

        secondRepository.Update(secondFolder);
        Func<Task> saveStaleFolder = () => secondScope.ServiceProvider
            .GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext))
            .SaveChangesAsync(cancellationToken);

        var exception = await saveStaleFolder.ShouldThrowAsync<ConcurrencyException>();
        exception.InnerException.ShouldBeOfType<DbUpdateConcurrencyException>();
        await using var verificationScope = Fixture.CreateScope();
        var persistedFolder = await verificationScope.ServiceProvider
            .GetRequiredService<IFoldersRepository>()
            .GetByIdAsync(folder.Id, cancellationToken);
        persistedFolder.ShouldNotBeNull();
        persistedFolder.Name.Value.ShouldBe("Players");
        persistedFolder.AllowedFileType.ShouldBe(folder.AllowedFileType);
        persistedFolder.ParentId.ShouldBe(folder.ParentId);
    }

    [Fact(DisplayName = "Folder repository should reject an orphan when parent does not exist")]
    public async Task Add_Should_RejectOrphan_When_ParentDoesNotExist()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var parent = Folder.Create(
            name: FolderName.Create("Avatars").Value,
            allowedFileType: FileType.Avatar,
            userId: null).Value;
        var child = parent.CreateChild(FolderName.Create("Players").Value);

        Func<Task> saveOrphan = () => SaveNewFolderAsync(child, cancellationToken);

        var exception = await saveOrphan.ShouldThrowAsync<DbUpdateException>();
        exception.InnerException.ShouldBeOfType<PostgresException>()
            .SqlState
            .ShouldBe(PostgresErrorCodes.ForeignKeyViolation);
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
            allowedFileType: FileType.Avatar,
            userId: null).Value;
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

    [Theory(
        DisplayName = "Folder repository should limit direct active children when nested and deleted folders exist")]
    [InlineData("Personal")]
    [InlineData("Avatar")]
    public async Task GetChildrenBatchAsync_Should_LimitDirectActiveChildren_When_NestedAndDeletedFoldersExist(
        string type)
    {
        var ct = TestContext.Current.CancellationToken;
        var root = Folder.Create(
            FolderName.Create("Root").Value,
            FileType.FromName(type),
            type == "Personal" ? UserId.Create(Guid.NewGuid()).Value : null).Value;
        var children = Enumerable.Range(0, 105)
            .Select(index => root.CreateChild(FolderName.Create($"Child {index}").Value))
            .ToList();
        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IFoldersRepository>();
            repository.Add(root);
            foreach (var child in children)
            {
                repository.Add(child);
            }
            repository.Add(children[0].CreateChild(FolderName.Create("Grandchild").Value));
            var unitOfWork = scope.ServiceProvider
                .GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext));
            await unitOfWork.SaveChangesAsync(ct);
            repository.Delete(children[104]);
            await unitOfWork.SaveChangesAsync(ct);
        }
        await using var verificationScope = Fixture.CreateScope();
        var folders = verificationScope.ServiceProvider.GetRequiredService<IFoldersRepository>();

        var loaded = await folders.GetChildrenBatchAsync(root.Id, ct);

        var expectedIds = children.Take(104).Select(folder => folder.Id.Value).Order().Take(100);
        loaded.Select(folder => folder.Id.Value).ShouldBe(expectedIds);
        var context = verificationScope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
        context.ChangeTracker.Entries<Folder>().Count().ShouldBe(100);
    }

    [Theory(DisplayName = "Folder reconciliation should find all pages when old deleted parents have active contents")]
    [InlineData("Personal")]
    [InlineData("Avatar")]
    public async Task GetDeletedWithActiveContentsAsync_Should_FindAllPages_When_OldDeletedParentsHaveActiveContents(
        string type)
    {
        var ct = TestContext.Current.CancellationToken;
        var owner = type == "Personal" ? UserId.Create(Guid.NewGuid()).Value : (UserId?)null;
        var parents = Enumerable.Range(0, 109)
            .Select(index => Folder.Create(
                FolderName.Create($"Parent {index}").Value, FileType.FromName(type), owner).Value)
            .ToArray();
        var deletedChild = parents[108].CreateChild(FolderName.Create("Deleted child").Value);
        var deletedFile = File.Create(FileName.Create("deleted.txt").Value, FileType.FromName(type), owner).Value;
        deletedFile.MoveTo(parents[108]);
        await using (var scope = Fixture.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
            db.AddRange(parents);
            for (var index = 0; index < 105; index++)
            {
                if (index % 3 == 0)
                {
                    db.Add(parents[index].CreateChild(FolderName.Create("Child").Value));
                }
                else
                {
                    var file = File.Create(
                        FileName.Create($"file-{index}.txt").Value, FileType.FromName(type), owner).Value;
                    file.MoveTo(parents[index]);
                    if (index % 3 == 2)
                    {
                        file.CompleteUpload(FileContentType.Create("text/plain").Value, FileSize.Create(1).Value);
                    }
                    db.Add(file);
                }
            }
            db.Add(parents[105].CreateChild(FolderName.Create("Active parent child").Value));
            var deletingFile = File.Create(
                FileName.Create("deleting.txt").Value, FileType.FromName(type), owner).Value;
            deletingFile.MoveTo(parents[107]);
            deletingFile.BeginDeletion();
            db.AddRange(deletingFile, deletedChild, deletedFile);
            await db.SaveChangesAsync(ct);
            var deletedIds = parents.Where((_, index) => index != 105)
                .Select(folder => folder.Id.Value)
                .Append(deletedChild.Id.Value)
                .ToArray();
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE file_manager.folders SET deleted_at = {DateTime.UnixEpoch} WHERE id = ANY({deletedIds})",
                ct);
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE file_manager.files SET deleted_at = {DateTime.UnixEpoch} WHERE id = {deletedFile.Id.Value}",
                ct);
        }
        await using var queryScope = Fixture.CreateScope();
        var repository = queryScope.ServiceProvider.GetRequiredService<IFoldersRepository>();
        var loaded = new List<Folder>();

        await foreach (var folder in repository.GetDeletedWithActiveContentsAsync(ct))
        {
            loaded.Add(folder);
        }

        var expectedIds = parents.Take(105).Select(folder => folder.Id.Value).Order();
        loaded.Select(folder => folder.Id.Value).ShouldBe(expectedIds);
        var context = queryScope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
        context.ChangeTracker.Entries().ShouldBeEmpty();
    }

    private async Task SaveNewFolderAsync(Folder folder, CancellationToken cancellationToken)
    {
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IFoldersRepository>();

        repository.Add(folder);
        await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext))
            .SaveChangesAsync(cancellationToken);
    }
}
