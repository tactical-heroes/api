using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;
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
            allowedFileType: FileType.Avatar);
        var repository = scope.ServiceProvider.GetRequiredService<IFoldersRepository>();
        await repository.AddAsync(folder, cancellationToken);

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
            allowedFileType: FileType.Avatar);

        var before = await readRepository.AnyAsync(cancellationToken);
        await repository.AddAsync(folder, cancellationToken);
        var after = await readRepository.AnyAsync(cancellationToken);

        before.ShouldBeFalse();
        after.ShouldBeTrue();
    }

    [Theory(DisplayName = "Folder read model should restore all columns without tracking when parent is optional")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ReadDbModel_Should_RestoreAllColumnsWithoutTracking_When_ParentIsOptional(bool nested)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var folder = Folder.Create(
            name: FolderName.Create("Avatars").Value,
            allowedFileType: FileType.Avatar);
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IFoldersRepository>();
        await repository.AddAsync(folder, cancellationToken);
        if (nested)
        {
            folder = folder.CreateChild(FolderName.Create("Players").Value);
            await repository.AddAsync(folder, cancellationToken);
        }

        var writeContext = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
        var persisted = (await writeContext.Entry(folder).GetDatabaseValuesAsync(cancellationToken))!;
        var readContext = scope.ServiceProvider.GetRequiredService<FileManagerReadDbContext>();

        var model = await readContext.Set<FolderReadDbModel>()
            .SingleAsync(row => row.Id == folder.Id.Value, cancellationToken);

        model.Id.ShouldBe(folder.Id.Value);
        model.Name.ShouldBe(folder.Name.Value);
        model.AllowedFileType.ShouldBe(folder.AllowedFileType.Name);
        model.ParentId.ShouldBe(folder.ParentId?.Value);
        model.CreatedAt.ShouldBe(persisted.GetValue<DateTime>("CreatedAt"));
        model.UpdatedAt.ShouldBe(persisted.GetValue<DateTime>("UpdatedAt"));
        model.DeletedAt.ShouldBeNull();
        readContext.ChangeTracker.Entries().ShouldBeEmpty();
    }

    [Theory(DisplayName = "Folder read model should load parent and direct children without tracking when depth varies")]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public async Task ReadDbModel_Should_LoadParentAndDirectChildrenWithoutTracking_When_DepthVaries(int depth)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var root = Folder.Create(
            name: FolderName.Create("Avatars").Value,
            allowedFileType: FileType.Avatar);
        var child = root.CreateChild(FolderName.Create("Players").Value);
        var grandchild = child.CreateChild(FolderName.Create("Heroes").Value);
        Folder[] folders = [root, child, grandchild];
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IFoldersRepository>();
        foreach (var folder in folders)
        {
            await repository.AddAsync(folder, cancellationToken);
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
}
