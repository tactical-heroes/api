using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.Core.Application.Persistence.Errors;

using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.IntegrationTests.Infrastructure.Persistence.Features.Files.Write;

public sealed class FileContentUpdatePersistenceTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact(DisplayName = "Updating query should return old active updates when updates exist")]
    public async Task GetUpdatingFilesAsync_Should_ReturnOldActiveUpdates_When_UpdatesExist()
    {
        var ct = TestContext.Current.CancellationToken;
        var old = CreateFile();
        var recent = CreateFile();
        var deleted = CreateFile();
        var ready = CreateFile();
        foreach (var file in new[] { old, recent, deleted })
        {
            file.BeginContentUpdate();
        }
        await using (var scope = Fixture.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
            db.AddRange(old, recent, deleted, ready);
            await db.SaveChangesAsync(ct);
            db.Remove(deleted);
            await db.SaveChangesAsync(ct);
            var then = DateTime.UtcNow.AddMinutes(-20);
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE file_manager.files SET updated_at = {then} WHERE id = {old.Id.Value} OR id = {deleted.Id.Value}",
                ct);
        }
        await using var verification = Fixture.CreateScope();
        var files = verification.ServiceProvider.GetRequiredService<IFilesRepository>();

        var result = new List<File>();
        await foreach (var file in files.GetUpdatingFilesAsync(ct))
        {
            result.Add(file);
        }

        result.ShouldHaveSingleItem().Id.ShouldBe(old.Id);
        result[0].PendingStorageKey.ShouldBe(old.PendingStorageKey);
        result[0].Status.ShouldBe(FileStatus.Updating);
        result[0].StorageKey.ShouldBe(old.StorageKey);
    }

    [Fact(DisplayName = "Only one content update should reserve a ready file when two writers race")]
    public async Task SaveChangesAsync_Should_RejectSecondReservation_When_TwoWritersRace()
    {
        var ct = TestContext.Current.CancellationToken;
        var file = CreateFile();
        await using (var scope = Fixture.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
            db.Add(file);
            await db.SaveChangesAsync(ct);
        }
        await using var first = Fixture.CreateScope();
        await using var second = Fixture.CreateScope();
        var firstRepository = first.ServiceProvider.GetRequiredService<IFilesRepository>();
        var secondRepository = second.ServiceProvider.GetRequiredService<IFilesRepository>();
        var firstFile = await firstRepository.GetByIdAsync(file.Id, ct);
        var secondFile = await secondRepository.GetByIdAsync(file.Id, ct);
        firstFile!.BeginContentUpdate();
        secondFile!.BeginContentUpdate();
        firstRepository.Update(firstFile);
        secondRepository.Update(secondFile);
        await first.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext))
            .SaveChangesAsync(ct);

        var exception = await Record.ExceptionAsync(() => second.ServiceProvider
            .GetRequiredKeyedService<IUnitOfWork>(typeof(FileManagerWriteDbContext))
            .SaveChangesAsync(ct));

        exception.ShouldBeOfType<ConcurrencyException>();
    }

    private static File CreateFile()
    {
        var file = File.Create(FileName.Create("file.txt").Value, FileType.Avatar, userId: null).Value;
        file.CompleteUpload(FileContentType.Create("text/plain").Value, FileSize.Create(4).Value);
        return file;
    }
}
