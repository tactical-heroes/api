using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Common.Filters;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Folders.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.FileManager.IntegrationTests.Infrastructure.Persistence.Features.Folders.Read;

public sealed class FoldersReadRepositoryTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact(DisplayName = "Folder read repository should find persisted identifiers when folder exists")]
    public async Task ExistsByIdAsync_Should_MatchPersistedIdentifiers_When_FolderExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var scope = Fixture.CreateScope();
        var readRepository = scope.ServiceProvider.GetRequiredService<IFoldersReadRepository>();
        var folder = Folder.Create(
            name: FolderName.Create("Avatars").Value,
            allowedFileType: FileType.Avatar,
            userId: null).Value;
        var repository = scope.ServiceProvider.GetRequiredService<IFoldersRepository>();
        repository.Add(folder);
        await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext))
            .SaveChangesAsync(cancellationToken);

        var exists = await readRepository.ExistsByIdAsync(folder.Id.Value, cancellationToken);
        var missing = await readRepository.ExistsByIdAsync(Guid.CreateVersion7(), cancellationToken);

        exists.ShouldBeTrue();
        missing.ShouldBeFalse();
    }

    [Fact(DisplayName = "Folder read repository should reflect persisted rows when folder is added")]
    public async Task AnyAsync_Should_ReflectPersistedRows_When_FolderIsAdded()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var scope = Fixture.CreateScope();
        var readRepository = scope.ServiceProvider.GetRequiredService<IFoldersReadRepository>();
        var repository = scope.ServiceProvider.GetRequiredService<IFoldersRepository>();
        var folder = Folder.Create(
            name: FolderName.Create("Avatars").Value,
            allowedFileType: FileType.Avatar,
            userId: null).Value;

        var before = await readRepository.AnyAsync(cancellationToken);
        repository.Add(folder);
        await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext))
            .SaveChangesAsync(cancellationToken);
        var after = await readRepository.AnyAsync(cancellationToken);

        before.ShouldBeFalse();
        after.ShouldBeTrue();
    }

    [Theory(
        DisplayName = "Folder read model should restore all columns without tracking when parent and ownership vary")]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task ReadDbModel_Should_RestoreAllColumnsWithoutTracking_When_ParentAndOwnershipVary(
        bool nested, bool personal)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        UserId? userId = personal ? UserId.Create(Guid.CreateVersion7()).Value : null;
        var folder = Folder.Create(
            name: FolderName.Create("Avatars").Value,
            allowedFileType: personal ? FileType.Personal : FileType.Avatar,
            userId: userId).Value;
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IFoldersRepository>();
        repository.Add(folder);
        await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext))
            .SaveChangesAsync(cancellationToken);
        if (nested)
        {
            folder = folder.CreateChild(FolderName.Create("Players").Value);
            repository.Add(folder);
            await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext))
                .SaveChangesAsync(cancellationToken);
        }

        var writeContext = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
        var persisted = (await writeContext.Entry(folder).GetDatabaseValuesAsync(cancellationToken))!;
        var readContext = scope.ServiceProvider.GetRequiredService<FileManagerReadDbContext>();

        var model = await readContext.Set<FolderReadDbModel>()
            .SingleAsync(row => row.Id == folder.Id.Value, cancellationToken);

        model.Id.ShouldBe(folder.Id.Value);
        model.Name.ShouldBe(folder.Name.Value);
        model.AllowedFileType.ShouldBe(folder.AllowedFileType.Name);
        model.UserId.ShouldBe(userId?.Value);
        model.ParentId.ShouldBe(folder.ParentId?.Value);
        model.CreatedAt.ShouldBe(persisted.GetValue<DateTime>("CreatedAt"));
        model.UpdatedAt.ShouldBe(persisted.GetValue<DateTime>("UpdatedAt"));
        model.DeletedAt.ShouldBeNull();
        readContext.ChangeTracker.Entries().ShouldBeEmpty();
    }

    [Theory(
        DisplayName = "Folder read model should load parent and direct children without tracking when depth varies")]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public async Task ReadDbModel_Should_LoadParentAndDirectChildrenWithoutTracking_When_DepthVaries(int depth)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var root = Folder.Create(
            name: FolderName.Create("Avatars").Value,
            allowedFileType: FileType.Avatar,
            userId: null).Value;
        var child = root.CreateChild(FolderName.Create("Players").Value);
        var grandchild = child.CreateChild(FolderName.Create("Heroes").Value);
        Folder[] folders = [root, child, grandchild];
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IFoldersRepository>();
        foreach (var folder in folders)
        {
            repository.Add(folder);
            await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext))
                .SaveChangesAsync(cancellationToken);
        }

        var readContext = scope.ServiceProvider.GetRequiredService<FileManagerReadDbContext>();
        var id = folders[depth].Id.Value;

        var model = await readContext.Set<FolderReadDbModel>()
            .Include(folder => folder.Parent)
            .Include(folder => folder.Children)
            .SingleAsync(folder => folder.Id == id, cancellationToken);

        (model.Parent?.Id).ShouldBe(folders[depth].ParentId?.Value);
        model.Children.Select(folder => folder.Id).ShouldBe(
            folders.Where(folder => folder.ParentId == folders[depth].Id)
                .Select(folder => folder.Id.Value));
        readContext.ChangeTracker.Entries().ShouldBeEmpty();
    }

    [Theory(DisplayName = "Folder repository should filter before paging when type and owner vary")]
    [InlineData("Personal", true, 2, "Gamma")]
    [InlineData("Personal", false, 4, "Beta")]
    [InlineData("Avatar", false, 1, "Avatar")]
    [InlineData(null, true, 3, "Avatar")]
    [InlineData(null, false, 5, "Avatar")]
    public async Task GetPageAsync_Should_FilterBeforePaging_When_TypeAndOwnerVary(
        string? type, bool own, long total, string name)
    {
        var userId = Guid.NewGuid();
        await AddFolderAsync("Alpha", "Personal", userId);
        await AddFolderAsync("Gamma", "Personal", userId);
        await AddFolderAsync("Beta", "Personal", Guid.NewGuid());
        await AddFolderAsync("Delta", "Personal", Guid.NewGuid());
        await AddFolderAsync("Avatar", "Avatar", null);
        await AddFolderAsync("Aardvark", "Personal", userId, deleted: true);
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IFoldersReadRepository>();

        var page = await repository.GetPageAsync(
            new FoldersFilter(AllowedFileType: type, UserId: own ? userId : null),
            new PaginationParameters(type == "Avatar" ? 1 : 2, 1),
            SortingParameters.None,
            TestContext.Current.CancellationToken);

        page.TotalCount.ShouldBe(total);
        page.Items.ShouldHaveSingleItem().Name.ShouldBe(name);
        scope.ServiceProvider.GetRequiredService<FileManagerReadDbContext>().ChangeTracker.Entries().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Folder repository should search inside the selection when search and type have whitespace")]
    public async Task GetPageAsync_Should_SearchInsideSelection_When_SearchAndTypeHaveWhitespace()
    {
        var userId = Guid.NewGuid();
        var folder = await AddFolderAsync("Annual REPORT", "Personal", userId);
        await AddFolderAsync("Foreign report", "Personal", Guid.NewGuid());
        await AddFolderAsync("Avatar report", "Avatar", null);
        await AddFolderAsync("Deleted report", "Personal", userId, deleted: true);
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IFoldersReadRepository>();

        var page = await repository.GetPageAsync(
            new FoldersFilter(Search: " RePoRt ", AllowedFileType: " Personal ", UserId: userId),
            new PaginationParameters(),
            SortingParameters.None,
            TestContext.Current.CancellationToken);

        page.TotalCount.ShouldBe(1);
        page.Items.ShouldHaveSingleItem().Id.ShouldBe(folder.Id.Value);
    }

    [Theory(DisplayName = "Folder repository should project parent metadata when parent visibility varies")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetPageAsync_Should_ProjectParentMetadata_When_ParentVisibilityVaries(bool parentDeleted)
    {
        var parent = await AddFolderAsync("Parent", "Avatar", null);
        var child = parent.CreateChild(FolderName.Create("Child").Value);
        await using var scope = Fixture.CreateScope();
        scope.ServiceProvider.GetRequiredService<IFoldersRepository>().Add(child);
        await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext))
            .SaveChangesAsync(TestContext.Current.CancellationToken);
        if (parentDeleted)
        {
            await using var deletionScope = Fixture.CreateScope();
            var writeRepository = deletionScope.ServiceProvider.GetRequiredService<IFoldersRepository>();
            var stored = await writeRepository.GetByIdAsync(parent.Id, TestContext.Current.CancellationToken);
            writeRepository.Delete(stored!);
            await deletionScope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext))
                .SaveChangesAsync(TestContext.Current.CancellationToken);
        }
        var repository = scope.ServiceProvider.GetRequiredService<IFoldersReadRepository>();

        var page = await repository.GetPageAsync(
            new FoldersFilter(AllowedFileType: "Avatar"),
            new PaginationParameters(),
            SortingParameters.None,
            TestContext.Current.CancellationToken);

        var item = page.Items.Single(folder => folder.Id == child.Id.Value);
        item.Name.ShouldBe(child.Name.Value);
        item.AllowedFileType.ShouldBe("Avatar");
        item.ParentId.ShouldBe(parent.Id.Value);
        item.ParentName.ShouldBe(parentDeleted ? null : parent.Name.Value);
        page.TotalCount.ShouldBe(parentDeleted ? 1 : 2);
        if (!parentDeleted)
        {
            var root = page.Items.Single(folder => folder.Id == parent.Id.Value);
            root.ParentId.ShouldBeNull();
            root.ParentName.ShouldBeNull();
        }
        scope.ServiceProvider.GetRequiredService<FileManagerReadDbContext>().ChangeTracker.Entries().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Folder repository should break ties by descending id when names are equal")]
    public async Task GetPageAsync_Should_BreakTiesByDescendingId_When_NamesAreEqual()
    {
        var first = await AddFolderAsync("Same", "Avatar", null);
        var second = await AddFolderAsync("Same", "Avatar", null);
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IFoldersReadRepository>();
        var filter = new FoldersFilter(AllowedFileType: "Avatar");

        var firstPage = await repository.GetPageAsync(
            filter,
            new PaginationParameters(1, 1),
            SortingParameters.None,
            TestContext.Current.CancellationToken);
        var secondPage = await repository.GetPageAsync(
            filter,
            new PaginationParameters(2, 1),
            SortingParameters.None,
            TestContext.Current.CancellationToken);

        var ids = new[] { first.Id.Value, second.Id.Value }.OrderDescending().ToArray();
        firstPage.Items.ShouldHaveSingleItem().Id.ShouldBe(ids[0]);
        secondPage.Items.ShouldHaveSingleItem().Id.ShouldBe(ids[1]);
    }

    [Theory(DisplayName = "Folder repository should preserve total when the requested page has no items")]
    [InlineData(0)]
    [InlineData(1)]
    public async Task GetPageAsync_Should_PreserveTotal_When_TheRequestedPageHasNoItems(int count)
    {
        for (var i = 0; i < count; i++)
        {
            await AddFolderAsync("Folder", "Avatar", null);
        }
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IFoldersReadRepository>();

        var page = await repository.GetPageAsync(
            new FoldersFilter(AllowedFileType: "Avatar"),
            new PaginationParameters(2, 1),
            SortingParameters.None,
            TestContext.Current.CancellationToken);

        page.TotalCount.ShouldBe(count);
        page.Items.ShouldBeEmpty();
    }

    [Theory(DisplayName = "Folder details should project persisted metadata when type and parent state vary")]
    [InlineData("Personal", "root")]
    [InlineData("Personal", "nested")]
    [InlineData("Personal", "deleted-parent")]
    [InlineData("Avatar", "root")]
    [InlineData("Avatar", "nested")]
    [InlineData("Avatar", "deleted-parent")]
    public async Task GetDetailsByIdAsync_Should_ProjectPersistedMetadata_When_TypeAndParentStateVary(
        string type, string parentState)
    {
        var userId = type == "Personal" ? Guid.NewGuid() : (Guid?)null;
        var parent = await AddFolderAsync("Parent", type, userId);
        var folder = parent;
        if (parentState != "root")
        {
            folder = parent.CreateChild(FolderName.Create("Child").Value);
            await using (var writeScope = Fixture.CreateScope())
            {
                writeScope.ServiceProvider.GetRequiredService<IFoldersRepository>().Add(folder);
                await writeScope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext))
                    .SaveChangesAsync(TestContext.Current.CancellationToken);
            }
            if (parentState == "deleted-parent")
            {
                await using var deletionScope = Fixture.CreateScope();
                var writeRepository = deletionScope.ServiceProvider.GetRequiredService<IFoldersRepository>();
                var stored = await writeRepository.GetByIdAsync(parent.Id, TestContext.Current.CancellationToken);
                writeRepository.Delete(stored!);
                await deletionScope.ServiceProvider
                    .GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext))
                    .SaveChangesAsync(TestContext.Current.CancellationToken);
            }
        }
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IFoldersReadRepository>();

        var details = await repository.GetDetailsByIdAsync(folder.Id.Value, TestContext.Current.CancellationToken);

        details.ShouldNotBeNull();
        details.Id.ShouldBe(folder.Id.Value);
        details.Name.ShouldBe(folder.Name.Value);
        details.AllowedFileType.ShouldBe(type);
        details.UserId.ShouldBe(userId);
        details.ParentId.ShouldBe(parentState == "root" ? null : parent.Id.Value);
        details.ParentName.ShouldBe(parentState == "nested" ? "Parent" : null);
        scope.ServiceProvider.GetRequiredService<FileManagerReadDbContext>().ChangeTracker.Entries().ShouldBeEmpty();
    }

    [Theory(DisplayName = "Folder details should be absent when folder is unknown or deleted")]
    [InlineData("Personal", false)]
    [InlineData("Personal", true)]
    [InlineData("Avatar", false)]
    [InlineData("Avatar", true)]
    public async Task GetDetailsByIdAsync_Should_BeAbsent_When_FolderIsUnknownOrDeleted(string type, bool deleted)
    {
        var folder = await AddFolderAsync("Folder", type, type == "Personal" ? Guid.NewGuid() : null, deleted);
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IFoldersReadRepository>();

        var details = await repository.GetDetailsByIdAsync(
            deleted ? folder.Id.Value : Guid.NewGuid(),
            TestContext.Current.CancellationToken);

        details.ShouldBeNull();
    }

    private async Task<Folder> AddFolderAsync(string name, string type, Guid? userId, bool deleted = false)
    {
        var folder = Folder.Create(
            FolderName.Create(name).Value,
            FileType.FromName(type),
            userId is { } owner ? UserId.Create(owner).Value : null).Value;
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IFoldersRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext));
        repository.Add(folder);
        await unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
        if (deleted)
        {
            repository.Delete(folder);
            await unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
        }
        return folder;
    }
}
