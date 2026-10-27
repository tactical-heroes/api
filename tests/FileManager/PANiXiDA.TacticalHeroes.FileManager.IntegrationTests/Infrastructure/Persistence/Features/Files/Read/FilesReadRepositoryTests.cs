using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Read.DbModels;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Folders.Read.DbModels;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.IntegrationTests.Infrastructure.Persistence.Features.Files.Read;

public sealed class FilesReadRepositoryTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Theory(DisplayName = "Personal file details should project persisted metadata when upload state varies")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetPersonalDetailsByIdAsync_Should_ProjectMetadata_When_UploadStateVaries(bool completed)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var file = await AddFileAsync(true, cancellationToken);
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IFilesRepository>();
        var stored = await repository.GetByIdAsync(file.Id, cancellationToken);
        stored.ShouldNotBeNull();
        var folder = Folder.Create(FolderName.Create("Documents").Value, FileType.Personal, file.UserId).Value;
        scope.ServiceProvider.GetRequiredService<IFoldersRepository>().Add(folder);
        stored.MoveTo(folder).IsSuccess.ShouldBeTrue();
        if (completed)
        {
            stored.CompleteUpload(FileContentType.Create("image/png").Value, FileSize.Create(4096).Value)
                .IsSuccess
                .ShouldBeTrue();
        }

        repository.Update(stored);
        await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext))
            .SaveChangesAsync(cancellationToken);
        var readRepository = scope.ServiceProvider.GetRequiredService<IFilesReadRepository>();

        var model = await readRepository.GetPersonalDetailsByIdAsync(
            file.Id.Value, file.UserId!.Value.Value, cancellationToken);

        model.ShouldNotBeNull();
        model.Id.ShouldBe(file.Id.Value);
        model.Name.ShouldBe(file.Name.Value);
        model.FolderId.ShouldBe(folder.Id.Value);
        model.Status.ShouldBe(completed ? FileStatus.Ready.Name : FileStatus.PendingUpload.Name);
        model.ContentType.ShouldBe(completed ? "image/png" : null);
        model.Size.ShouldBe(completed ? 4096L : null);
        scope.ServiceProvider.GetRequiredService<FileManagerReadDbContext>()
            .ChangeTracker
            .Entries()
            .ShouldBeEmpty();
    }

    [Theory(DisplayName = "Personal file details should be absent when file is unavailable")]
    [InlineData("missing")]
    [InlineData("foreign")]
    [InlineData("avatar")]
    [InlineData("deleted")]
    public async Task GetPersonalDetailsByIdAsync_Should_ReturnNull_When_FileIsUnavailable(string fileKind)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var file = await AddFileAsync(fileKind != "avatar", cancellationToken);
        var id = fileKind == "missing" ? Guid.NewGuid() : file.Id.Value;
        var userId = fileKind is "missing" or "deleted" ? file.UserId!.Value.Value : Guid.NewGuid();
        await using var scope = Fixture.CreateScope();
        if (fileKind == "deleted")
        {
            var writeRepository = scope.ServiceProvider.GetRequiredService<IFilesRepository>();
            var stored = await writeRepository.GetByIdAsync(file.Id, cancellationToken);
            writeRepository.Delete(stored!);
            await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext))
                .SaveChangesAsync(cancellationToken);
        }

        var repository = scope.ServiceProvider.GetRequiredService<IFilesReadRepository>();

        var model = await repository.GetPersonalDetailsByIdAsync(id, userId, cancellationToken);

        model.ShouldBeNull();
    }

    [Fact(DisplayName = "ExistsByIdAsync should match persisted identifiers when file exists")]
    public async Task ExistsByIdAsync_Should_MatchPersistedIdentifiers_When_FileExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var file = await AddFileAsync(false, cancellationToken);
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IFilesReadRepository>();

        var exists = await repository.ExistsByIdAsync(file.Id.Value, cancellationToken);
        var missing = await repository.ExistsByIdAsync(Guid.CreateVersion7(), cancellationToken);

        exists.ShouldBeTrue();
        missing.ShouldBeFalse();
    }

    [Fact(DisplayName = "AnyAsync should reflect persisted rows when files are added")]
    public async Task AnyAsync_Should_ReflectPersistedRows_When_FilesAreAdded()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IFilesReadRepository>();

        var before = await repository.AnyAsync(cancellationToken);
        await AddFileAsync(false, cancellationToken);
        var after = await repository.AnyAsync(cancellationToken);

        before.ShouldBeFalse();
        after.ShouldBeTrue();
    }

    [Theory(
        DisplayName = "Read database model should restore all columns without tracking when upload state and ownership vary")]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task ReadDbModel_Should_RestoreAllColumnsWithoutTracking_When_UploadStateAndOwnershipVary(
        bool completed, bool personal)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var file = await AddFileAsync(personal, cancellationToken);
        await using var scope = Fixture.CreateScope();
        var writeContext = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IFilesRepository>();
        var stored = await repository.GetByIdAsync(file.Id, cancellationToken);
        stored.ShouldNotBeNull();
        stored.UserId.ShouldBe(file.UserId);
        stored.StorageKey.ShouldBe(file.StorageKey);
        if (completed)
        {
            stored.CompleteUpload(
                FileContentType.Create("image/png").Value,
                FileSize.Create(4096).Value);
            repository.Update(stored);
            await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext))
                .SaveChangesAsync(cancellationToken);
        }

        var entry = writeContext.Entry(stored);
        var persisted = (await entry.GetDatabaseValuesAsync(cancellationToken))!;
        var readContext = scope.ServiceProvider.GetRequiredService<FileManagerReadDbContext>();

        var model = await readContext.Set<FileReadDbModel>()
            .SingleAsync(row => row.Id == file.Id.Value, cancellationToken);

        model.Id.ShouldBe(file.Id.Value);
        model.Name.ShouldBe(file.Name.Value);
        model.StorageKey.ShouldBe(file.StorageKey.Value);
        model.Type.ShouldBe(file.Type.Name);
        model.UserId.ShouldBe(file.UserId?.Value);
        model.FolderId.ShouldBeNull();
        model.Status.ShouldBe(completed ? FileStatus.Ready.Name : FileStatus.PendingUpload.Name);
        model.ContentType.ShouldBe(completed ? "image/png" : null);
        model.Size.ShouldBe(completed ? 4096L : null);
        model.CreatedAt.ShouldBe(persisted.GetValue<DateTime>("CreatedAt"));
        model.UpdatedAt.ShouldBe(persisted.GetValue<DateTime>("UpdatedAt"));
        model.DeletedAt.ShouldBeNull();
        readContext.ChangeTracker.Entries().ShouldBeEmpty();
    }

    [Theory(
        DisplayName = "Read models should load file and folder navigations without tracking when folder is optional")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ReadDbModel_Should_LoadFileAndFolderNavigationsWithoutTracking_When_FolderIsOptional(
        bool assigned)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var file = await AddFileAsync(false, cancellationToken);
        var otherFile = await AddFileAsync(false, cancellationToken);
        var folder = Folder.Create(
            name: FolderName.Create("Avatars").Value,
            allowedFileType: FileType.Avatar,
            userId: null).Value;
        await using var scope = Fixture.CreateScope();
        scope.ServiceProvider.GetRequiredService<IFoldersRepository>().Add(folder);
        await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext))
            .SaveChangesAsync(cancellationToken);
        if (assigned)
        {
            var repository = scope.ServiceProvider.GetRequiredService<IFilesRepository>();
            var stored = await repository.GetByIdAsync(file.Id, cancellationToken);
            stored!.MoveTo(folder).IsSuccess.ShouldBeTrue();
            repository.Update(stored);
            await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext))
                .SaveChangesAsync(cancellationToken);
        }

        var context = scope.ServiceProvider.GetRequiredService<FileManagerReadDbContext>();

        var fileModel = await context.Set<FileReadDbModel>()
            .Include(item => item.Folder)
            .SingleAsync(item => item.Id == file.Id.Value, cancellationToken);
        var folderModel = await context.Set<FolderReadDbModel>()
            .Include(item => item.Files)
            .SingleAsync(item => item.Id == folder.Id.Value, cancellationToken);

        fileModel.FolderId.ShouldBe(assigned ? folder.Id.Value : null);
        (fileModel.Folder?.Id).ShouldBe(fileModel.FolderId);
        folderModel.Files.Select(item => item.Id).ShouldBe(assigned ? [file.Id.Value] : []);
        folderModel.Files.ShouldNotContain(item => item.Id == otherFile.Id.Value);
        context.ChangeTracker.Entries().ShouldBeEmpty();
    }

    private async Task<File> AddFileAsync(bool personal, CancellationToken cancellationToken)
    {
        var file = File.Create(
            FileName.Create("avatar.png").Value,
            personal ? FileType.Personal : FileType.Avatar,
            personal ? UserId.Create(Guid.CreateVersion7()).Value : null).Value;
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IFilesRepository>();
        repository.Add(file);
        await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext))
            .SaveChangesAsync(cancellationToken);

        return file;
    }
}
