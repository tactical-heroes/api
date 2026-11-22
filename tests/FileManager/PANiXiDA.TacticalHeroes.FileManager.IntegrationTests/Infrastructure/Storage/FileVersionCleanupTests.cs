using System.Collections.ObjectModel;

using Amazon.S3.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using PANiXiDA.Core.Application.Storage;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.CleanupVersion;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Write;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Scheduling.Options.FileUploadReconciliation;
using PANiXiDA.TacticalHeroes.Testing.Storage;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.IntegrationTests.Infrastructure.Storage;

public sealed class FileVersionCleanupTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Theory(DisplayName = "Version cleanup should remove retired history when newer versions vary")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_RemoveRetiredHistory_When_NewerVersionsVary(bool newerVersions)
    {
        var ct = TestContext.Current.CancellationToken;
        await using var scope = Fixture.CreateScope();
        var services = scope.ServiceProvider;
        var storage = services.GetRequiredService<IFileStorage>();
        var db = services.GetRequiredService<FileManagerWriteDbContext>();
        var file = File.Create(FileName.Create("history.txt").Value, FileType.Avatar, null).Value;
        var versions = new List<string>
        {
            await UploadVersionAsync(storage, file.StorageKey.Value, 1, ct),
            await UploadVersionAsync(storage, file.StorageKey.Value, 2, ct)
        };
        if (newerVersions)
        {
            versions.Add(await UploadVersionAsync(storage, file.StorageKey.Value, 3, ct));
            versions.Add(await UploadVersionAsync(storage, file.StorageKey.Value, 4, ct));
            file.CompleteUpload(
                FileContentType.Create("text/plain").Value,
                FileSize.Create(1).Value,
                FileVersionId.Create(versions[^1]).Value)
                .IsSuccess
                .ShouldBeTrue();
            db.Add(file);
            await db.SaveChangesAsync(ct);
        }

        var repository = new FilesRepository(
            db,
            services.GetRequiredService<IOptions<FileUploadReconciliationOptions>>(),
            TimeProvider.System);
        var handler = new CleanupFileVersionHandler(repository, storage);
        var command = new CleanupFileVersionCommand(file.Id.Value, file.StorageKey.Value, versions[1]);

        var result = await handler.HandleAsync(command, ct);
        var remaining = await storage.GetVersionsAsync(file.StorageKey.Value, ct)
            .Select(version => version.VersionId)
            .ToListAsync(ct);

        result.IsSuccess.ShouldBeTrue();
        string[] expected = newerVersions ? [versions[3], versions[2]] : [];
        remaining.ShouldBe(expected);
        (await storage.GetMetadataAsync(file.StorageKey.Value, versions[0], ct)).ShouldBeNull();
        (await storage.GetMetadataAsync(file.StorageKey.Value, versions[1], ct)).ShouldBeNull();
        if (newerVersions)
        {
            var current = await storage.GetMetadataAsync(file.StorageKey.Value, ct);
            current.ShouldNotBeNull();
            current.VersionId.ShouldBe(versions[3]);
            db.ChangeTracker.Clear();
            var stored = await db.Set<File>().SingleAsync(item => item.Id == file.Id, ct);
            stored.VersionId!.Value.ShouldBe(versions[3]);
        }
        else
        {
            (await storage.GetMetadataAsync(file.StorageKey.Value, ct)).ShouldBeNull();
        }
    }

    [Fact(DisplayName = "Version listing should retain chronological order when uploads occur between pages")]
    public async Task GetVersionsAsync_Should_RetainChronologicalOrder_When_UploadsOccurBetweenPages()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var scope = Fixture.CreateScope();
        var storage = scope.ServiceProvider.GetRequiredService<IFileStorage>();
        var file = File.Create(FileName.Create("ordering.txt").Value, FileType.Avatar, null).Value;
        var versions = new List<string>
        {
            await UploadVersionAsync(storage, file.StorageKey.Value, 1, ct),
            await UploadVersionAsync(storage, file.StorageKey.Value, 2, ct)
        };
        await storage.DeleteAsync(file.StorageKey.Value, ct);
        versions.Add(await UploadVersionAsync(storage, file.StorageKey.Value, 3, ct));
        versions.Add(await UploadVersionAsync(storage, file.StorageKey.Value, 4, ct));
        using var client = Fixture.Storage.CreateClient();
        var objectKey = $"{S3TestStorage.KeyPrefix}/{file.StorageKey.Value}";
        var request = new ListVersionsRequest
        {
            BucketName = S3TestStorage.BucketName,
            Prefix = objectKey,
            MaxKeys = 2
        };
        var listed = new List<string>();
        string? newVersion = null;
        var pages = 0;
        while (true)
        {
            var response = await client.ListVersionsAsync(request, ct);
            pages++;
            listed.AddRange((response.Versions ?? [])
                .Where(version => version.Key == objectKey && version.IsDeleteMarker != true)
                .Select(version => version.VersionId));
            if (pages == 1)
            {
                response.IsTruncated.ShouldBe(true);
                newVersion = await UploadVersionAsync(storage, file.StorageKey.Value, 5, ct);
            }

            if (response.IsTruncated != true)
            {
                break;
            }

            request.KeyMarker = response.NextKeyMarker;
            request.VersionIdMarker = response.NextVersionIdMarker;
        }

        listed.ShouldBe(versions.AsEnumerable().Reverse());
        pages.ShouldBeGreaterThan(1);
        newVersion.ShouldNotBeNull();
        var all = await storage.GetVersionsAsync(file.StorageKey.Value, ct)
            .Select(version => version.VersionId)
            .ToListAsync(ct);
        string[] expected = [newVersion, .. versions.AsEnumerable().Reverse()];
        all.ShouldBe(expected);
    }

    private static async Task<string> UploadVersionAsync(
        IFileStorage storage,
        string key,
        byte value,
        CancellationToken cancellationToken)
    {
        using var content = new MemoryStream([value]);
        var uploaded = await storage.UploadAsync(
            key: key,
            content: content,
            contentType: "text/plain",
            metadata: ReadOnlyDictionary<string, string>.Empty,
            cancellationToken: cancellationToken);

        return FileVersionId.Create(uploaded.VersionId).Value.Value;
    }
}
