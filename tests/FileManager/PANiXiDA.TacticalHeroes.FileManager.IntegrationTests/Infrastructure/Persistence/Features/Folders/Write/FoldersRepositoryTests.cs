using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using PANiXiDA.Core.Application.Persistence.Errors;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
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

    [Theory(DisplayName = "Folder writes should reject a stale child entry when parent was deleted")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SaveChangesAsync_Should_RejectStaleChildEntry_When_ParentWasDeleted(bool fileEntry)
    {
        var ct = TestContext.Current.CancellationToken;
        var root = Folder.Create(FolderName.Create("Root").Value, FileType.Avatar, userId: null).Value;
        await SaveNewFolderAsync(root, ct);
        await using var writerScope = Fixture.CreateScope();
        var writer = writerScope.ServiceProvider.GetRequiredService<IFoldersRepository>();
        var parent = await writer.GetByIdAsync(root.Id, ct);
        await using (var deletingScope = Fixture.CreateScope())
        {
            var deleting = deletingScope.ServiceProvider.GetRequiredService<IFoldersRepository>();
            var stored = await deleting.GetByIdAsync(root.Id, ct);
            deleting.Delete(stored!);
            await deletingScope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext))
                .SaveChangesAsync(ct);
        }
        if (fileEntry)
        {
            var file = File.Create(FileName.Create("avatar.png").Value, FileType.Avatar, userId: null).Value;
            file.MoveTo(parent!);
            writerScope.ServiceProvider.GetRequiredService<IFilesRepository>().Add(file);
        }
        else
        {
            writer.Add(parent!.CreateChild(FolderName.Create("Child").Value));
        }
        writer.Update(parent!);

        Func<Task> save = () => writerScope.ServiceProvider
            .GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext))
            .SaveChangesAsync(ct);

        await save.ShouldThrowAsync<ConcurrencyException>();
        await using var checkScope = Fixture.CreateScope();
        var db = checkScope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
        (await db.Set<File>().AnyAsync(ct)).ShouldBeFalse();
        (await db.Set<Folder>().AnyAsync(ct)).ShouldBeFalse();
    }

    [Fact(DisplayName = "Folder deletion should reject a stale delete when a child entry was saved first")]
    public async Task SaveChangesAsync_Should_RejectStaleDelete_When_AChildEntryWasSavedFirst()
    {
        var ct = TestContext.Current.CancellationToken;
        var root = Folder.Create(FolderName.Create("Root").Value, FileType.Avatar, userId: null).Value;
        await SaveNewFolderAsync(root, ct);
        await using var deletingScope = Fixture.CreateScope();
        var deleting = deletingScope.ServiceProvider.GetRequiredService<IFoldersRepository>();
        var stale = await deleting.GetByIdAsync(root.Id, ct);
        await using (var creatingScope = Fixture.CreateScope())
        {
            var creating = creatingScope.ServiceProvider.GetRequiredService<IFoldersRepository>();
            var parent = await creating.GetByIdAsync(root.Id, ct);
            creating.Add(parent!.CreateChild(FolderName.Create("Child").Value));
            creating.Update(parent);
            await creatingScope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext))
                .SaveChangesAsync(ct);
        }
        deleting.Delete(stale!);

        Func<Task> save = () => deletingScope.ServiceProvider
            .GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext))
            .SaveChangesAsync(ct);

        await save.ShouldThrowAsync<ConcurrencyException>();
        await using var scope = Fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
        (await db.Set<Folder>().CountAsync(ct)).ShouldBe(2);
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
