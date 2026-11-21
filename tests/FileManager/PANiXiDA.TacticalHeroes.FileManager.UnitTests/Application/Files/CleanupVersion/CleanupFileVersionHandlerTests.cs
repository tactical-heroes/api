using PANiXiDA.Core.Application.Storage;

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
}
