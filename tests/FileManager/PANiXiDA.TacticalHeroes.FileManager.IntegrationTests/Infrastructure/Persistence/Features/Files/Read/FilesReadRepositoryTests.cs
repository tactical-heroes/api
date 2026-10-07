using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Read.DbModels;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.IntegrationTests.Infrastructure.Persistence.Features.Files.Read;

public sealed class FilesReadRepositoryTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact(DisplayName = "ExistsByIdAsync should match persisted identifiers when file exists")]
    public async Task ExistsByIdAsync_Should_MatchPersistedIdentifiers_When_FileExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var file = await AddFileAsync(cancellationToken);
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
        await AddFileAsync(cancellationToken);
        var after = await repository.AnyAsync(cancellationToken);

        before.ShouldBeFalse();
        after.ShouldBeTrue();
    }

    [Theory(DisplayName = "Read database model should restore all columns without tracking when upload state varies")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ReadDbModel_Should_RestoreAllColumnsWithoutTracking_When_UploadStateVaries(bool completed)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var file = await AddFileAsync(cancellationToken);
        await using var scope = Fixture.CreateScope();
        var writeContext = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IFilesRepository>();
        var stored = await repository.GetByIdAsync(file.Id, cancellationToken);
        if (completed)
        {
            stored!.CompleteUpload(
                FileContentType.Create("image/png").Value,
                FileSize.Create(4096).Value);
            await repository.UpdateAsync(stored, cancellationToken);
        }

        var entry = writeContext.Entry(stored!);
        var persisted = (await entry.GetDatabaseValuesAsync(cancellationToken))!;
        var readContext = scope.ServiceProvider.GetRequiredService<FileManagerReadDbContext>();

        var model = await readContext.Set<FileReadDbModel>()
            .SingleAsync(row => row.Id == file.Id.Value, cancellationToken);

        model.Id.ShouldBe(file.Id.Value);
        model.Name.ShouldBe(file.Name.Value);
        model.Type.ShouldBe(file.Type.Name);
        model.Status.ShouldBe(completed ? FileStatus.Ready.Name : FileStatus.PendingUpload.Name);
        model.ContentType.ShouldBe(completed ? "image/png" : null);
        model.Size.ShouldBe(completed ? 4096L : null);
        model.CreatedAt.ShouldBe(persisted.GetValue<DateTime>("CreatedAt"));
        model.UpdatedAt.ShouldBe(persisted.GetValue<DateTime>("UpdatedAt"));
        model.DeletedAt.ShouldBeNull();
        model.Version.ShouldBe(entry.Property<uint>("Version").CurrentValue);
        model.Version.ShouldBeGreaterThan(0u);
        readContext.ChangeTracker.Entries().ShouldBeEmpty();
    }

    private async Task<File> AddFileAsync(CancellationToken cancellationToken)
    {
        var file = File.Create(
            FileName.Create("avatar.png").Value,
            FileType.Avatar);
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IFilesRepository>();
        await repository.AddAsync(file, cancellationToken);

        return file;
    }
}
