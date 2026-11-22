using PANiXiDA.Core.Application.Storage;
using PANiXiDA.Core.Application.Storage.Models;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.CleanupVersion;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Files.CleanupVersion;

public sealed class CleanupFileVersionHandlerTests
{
    [Theory(DisplayName = "Version cleanup should delete only retired content when versions vary")]
    [InlineData("same-key", "active", false)]
    [InlineData("same-key", "retired", true)]
    [InlineData("old-key", "active", true)]
    public async Task HandleAsync_Should_DeleteOnlyRetiredContent_When_VersionsVary(
        string key,
        string version,
        bool deleted)
    {
        var ct = TestContext.Current.CancellationToken;
        var file = File.Create(FileName.Create("a.txt").Value, FileType.Avatar, null).Value;
        file.CompleteUpload(
            FileContentType.Create("text/plain").Value,
            FileSize.Create(4).Value,
            FileVersionId.Create("active").Value);
        var files = Substitute.For<IFilesRepository>();
        var storage = Substitute.For<IFileStorage>();
        files.GetByIdAsync(file.Id, ct).Returns(file);
        var command = new CleanupFileVersionCommand(
            file.Id.Value,
            key == "same-key" ? file.StorageKey.Value : "old-key",
            version);

        storage.GetVersionsAsync(command.StorageKey, ct)
            .Returns(new[] { new FileVersion(version, DateTimeOffset.UnixEpoch) }.ToAsyncEnumerable());

        var result = await new CleanupFileVersionHandler(files, storage).HandleAsync(command, ct);

        result.IsSuccess.ShouldBeTrue();
        await storage.Received(deleted ? 1 : 0).DeleteAsync(command.StorageKey, version, ct);
        await storage.DidNotReceive().DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        files.DidNotReceiveWithAnyArgs().Update(default!);
    }

    [Fact(DisplayName = "Version cleanup should avoid storage when command is invalid")]
    public async Task HandleAsync_Should_AvoidStorage_When_CommandIsInvalid()
    {
        var storage = Substitute.For<IFileStorage>();
        var files = Substitute.For<IFilesRepository>();

        var result = await new CleanupFileVersionHandler(files, storage).HandleAsync(
            new CleanupFileVersionCommand(Guid.Empty, "", ""),
            TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        storage.ReceivedCalls().ShouldBeEmpty();
        files.ReceivedCalls().ShouldBeEmpty();
    }

    [Theory(DisplayName = "Version cleanup should preserve newer content when a retired boundary exists")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_PreserveNewerContent_When_ARetiredBoundaryExists(bool fileMissing)
    {
        var ct = TestContext.Current.CancellationToken;
        var file = CreateReadyFile();
        var files = Substitute.For<IFilesRepository>();
        var storage = Substitute.For<IFileStorage>();
        if (!fileMissing)
        {
            files.GetByIdAsync(file.Id, ct).Returns(file);
        }

        var command = new CleanupFileVersionCommand(file.Id.Value, file.StorageKey.Value, "retired");
        storage.GetVersionsAsync(command.StorageKey, ct)
            .Returns(CreateVersions("newest", "active", "retired", "older-1", "older-2"));

        var result = await new CleanupFileVersionHandler(files, storage).HandleAsync(command, ct);

        result.IsSuccess.ShouldBeTrue();
        GetDeletedVersions(storage).ShouldBe(["older-1", "older-2", "retired"]);
        await storage.DidNotReceive().DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        files.DidNotReceiveWithAnyArgs().Update(default!);
        file.VersionId!.Value.ShouldBe("active");
    }

    [Fact(DisplayName = "Version cleanup should avoid deletion when the retired boundary is missing")]
    public async Task HandleAsync_Should_AvoidDeletion_When_TheRetiredBoundaryIsMissing()
    {
        var ct = TestContext.Current.CancellationToken;
        var storage = Substitute.For<IFileStorage>();
        storage.GetVersionsAsync("avatar/file", ct).Returns(CreateVersions("newest", "older"));

        var result = await new CleanupFileVersionHandler(Substitute.For<IFilesRepository>(), storage).HandleAsync(
            new CleanupFileVersionCommand(Guid.NewGuid(), "avatar/file", "missing"),
            ct);

        result.IsSuccess.ShouldBeTrue();
        GetDeletedVersions(storage).ShouldBeEmpty();
    }

    [Fact(DisplayName = "Version cleanup should preserve active content when it appears in older history")]
    public async Task HandleAsync_Should_PreserveActiveContent_When_ItAppearsInOlderHistory()
    {
        var ct = TestContext.Current.CancellationToken;
        var file = CreateReadyFile();
        var files = Substitute.For<IFilesRepository>();
        var storage = Substitute.For<IFileStorage>();
        files.GetByIdAsync(file.Id, ct).Returns(file);
        storage.GetVersionsAsync(file.StorageKey.Value, ct).Returns(CreateVersions("retired", "active", "older"));

        var result = await new CleanupFileVersionHandler(files, storage).HandleAsync(
            new CleanupFileVersionCommand(file.Id.Value, file.StorageKey.Value, "retired"),
            ct);

        result.IsSuccess.ShouldBeTrue();
        GetDeletedVersions(storage).ShouldBe(["older", "retired"]);
    }

    [Fact(DisplayName = "Version cleanup should retry remaining history when deletion was interrupted")]
    public async Task HandleAsync_Should_RetryRemainingHistory_When_DeletionWasInterrupted()
    {
        var ct = TestContext.Current.CancellationToken;
        var storage = Substitute.For<IFileStorage>();
        var versions = new List<string> { "newer", "retired", "older-1", "older-2" };
        storage.GetVersionsAsync("avatar/file", ct).Returns(_ => CreateVersions([.. versions]));
        var failure = new IOException("Storage deletion failed.");
        var failOnce = true;
        storage.DeleteAsync("avatar/file", Arg.Any<string>(), ct).Returns(call =>
        {
            var versionId = call.ArgAt<string>(1);
            if (versionId == "older-2" && failOnce)
            {
                failOnce = false;
                versions.Insert(0, "newest");
                throw failure;
            }

            versions.Remove(versionId);
            return Task.CompletedTask;
        });
        var handler = new CleanupFileVersionHandler(Substitute.For<IFilesRepository>(), storage);
        var command = new CleanupFileVersionCommand(Guid.NewGuid(), "avatar/file", "retired");

        var exception = await Record.ExceptionAsync(() => handler.HandleAsync(command, ct));

        exception.ShouldBeSameAs(failure);
        versions.ShouldContain("retired");
        versions.ShouldNotContain("older-1");

        var result = await handler.HandleAsync(command, ct);

        result.IsSuccess.ShouldBeTrue();
        versions.ShouldBe(["newest", "newer"]);
        GetDeletedVersions(storage).ShouldBe(["older-1", "older-2", "older-2", "retired"]);

        result = await handler.HandleAsync(command, ct);

        result.IsSuccess.ShouldBeTrue();
        GetDeletedVersions(storage).ShouldBe(["older-1", "older-2", "older-2", "retired"]);
    }

    [Fact(DisplayName = "Version cleanup should avoid partial deletion when version listing fails")]
    public async Task HandleAsync_Should_AvoidPartialDeletion_When_VersionListingFails()
    {
        var ct = TestContext.Current.CancellationToken;
        var storage = Substitute.For<IFileStorage>();
        var failure = new IOException("Listing failed.");
        storage.GetVersionsAsync("avatar/file", ct).Returns(ReadIncompleteVersionsAsync(failure));

        var exception = await Record.ExceptionAsync(() =>
            new CleanupFileVersionHandler(Substitute.For<IFilesRepository>(), storage).HandleAsync(
                new CleanupFileVersionCommand(Guid.NewGuid(), "avatar/file", "retired"),
                ct));

        exception.ShouldBeSameAs(failure);
        GetDeletedVersions(storage).ShouldBeEmpty();
    }

    [Fact(DisplayName = "Version cleanup should propagate cancellation when version listing is canceled")]
    public async Task HandleAsync_Should_PropagateCancellation_When_VersionListingIsCanceled()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var storage = Substitute.For<IFileStorage>();
        storage.GetVersionsAsync("avatar/file", cancellation.Token).Returns(ReadCanceledVersionsAsync(cancellation));

        var exception = await Record.ExceptionAsync(() =>
            new CleanupFileVersionHandler(Substitute.For<IFilesRepository>(), storage).HandleAsync(
                new CleanupFileVersionCommand(Guid.NewGuid(), "avatar/file", "retired"),
                cancellation.Token));

        exception.ShouldBeAssignableTo<OperationCanceledException>();
        GetDeletedVersions(storage).ShouldBeEmpty();
    }

    private static File CreateReadyFile()
    {
        var file = File.Create(FileName.Create("a.txt").Value, FileType.Avatar, null).Value;
        file.CompleteUpload(
            FileContentType.Create("text/plain").Value,
            FileSize.Create(4).Value,
            FileVersionId.Create("active").Value);

        return file;
    }

    private static IAsyncEnumerable<FileVersion> CreateVersions(params string[] versions)
    {
        return versions.Select(version => new FileVersion(version, DateTimeOffset.UnixEpoch)).ToAsyncEnumerable();
    }

    private static IEnumerable<string?> GetDeletedVersions(IFileStorage storage)
    {
        return storage.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(IFileStorage.DeleteAsync))
            .Select(call => call.GetArguments()[1] as string);
    }

    private static async IAsyncEnumerable<FileVersion> ReadCanceledVersionsAsync(CancellationTokenSource cancellation)
    {
        await cancellation.CancelAsync();
        cancellation.Token.ThrowIfCancellationRequested();
        yield break;
    }

    private static async IAsyncEnumerable<FileVersion> ReadIncompleteVersionsAsync(IOException failure)
    {
        yield return new FileVersion("retired", DateTimeOffset.UnixEpoch);
        await Task.Yield();
        yield return new FileVersion("older", DateTimeOffset.UnixEpoch);
        throw failure;
    }
}
