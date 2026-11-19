using NSubstitute.ExceptionExtensions;

using PANiXiDA.Core.Application.Storage;
using PANiXiDA.Core.Application.Storage.Models;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.DeleteStored;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Application.Files.DeleteStored;

public sealed class DeleteStoredFileHandlerTests
{
    [Fact(DisplayName = "Stored file cleanup should schedule rechecks when deletion succeeds")]
    public async Task HandleAsync_Should_ScheduleRechecks_When_DeletionSucceeds()
    {
        var storage = Substitute.For<IFileStorage>();
        var scheduler = Substitute.For<IFileContentCleanupScheduler>();
        var command = new DeleteStoredFileCommand("contents/unused", DateTimeOffset.UtcNow.AddDays(1));
        storage.GetMetadataAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FileMetadata("text/plain", 1));
        var handler = new DeleteStoredFileHandler(storage, scheduler);

        var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        await storage.Received(1).DeleteAsync(command.StorageKey, TestContext.Current.CancellationToken);
        await scheduler.Received(1).RescheduleAsync(command, TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Stored file cleanup should retain rechecks when the object is absent")]
    public async Task HandleAsync_Should_RetainRechecks_When_TheObjectIsAbsent()
    {
        var storage = Substitute.For<IFileStorage>();
        var scheduler = Substitute.For<IFileContentCleanupScheduler>();
        var handler = new DeleteStoredFileHandler(storage, scheduler);
        var command = new DeleteStoredFileCommand("contents/unused", DateTimeOffset.UtcNow.AddDays(1));

        var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        await storage.DidNotReceiveWithAnyArgs().DeleteAsync(default!, TestContext.Current.CancellationToken);
        await scheduler.Received(1).RescheduleAsync(command, TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Stored file cleanup should propagate failures when deletion fails")]
    public async Task HandleAsync_Should_PropagateFailures_When_DeletionFails()
    {
        var storage = Substitute.For<IFileStorage>();
        var scheduler = Substitute.For<IFileContentCleanupScheduler>();
        storage.GetMetadataAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FileMetadata("text/plain", 1));
        storage.DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).ThrowsAsync(new IOException());
        var handler = new DeleteStoredFileHandler(storage, scheduler);
        var command = new DeleteStoredFileCommand("contents/unused", DateTimeOffset.UtcNow.AddDays(1));

        var exception = await Record.ExceptionAsync(() =>
            handler.HandleAsync(command, TestContext.Current.CancellationToken));

        exception.ShouldBeOfType<IOException>();
        scheduler.ReceivedCalls().ShouldBeEmpty();
    }
}
