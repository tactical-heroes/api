using NSubstitute;

using PANiXiDA.Core.Application.Messaging.Scheduling;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.DeleteStored;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Scheduling.Options.FileUploadReconciliation;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Scheduling.Reconciliation;

namespace PANiXiDA.TacticalHeroes.FileManager.IntegrationTests.Infrastructure.Scheduling.Reconciliation;

public sealed class FileContentCleanupSchedulerTests
{
    [Fact(DisplayName = "Cleanup scheduler should retain the exact key when cleanup is scheduled")]
    public async Task ScheduleAsync_Should_RetainExactKey_When_CleanupIsScheduled()
    {
        var now = DateTimeOffset.UtcNow;
        var clock = Substitute.For<TimeProvider>();
        clock.GetUtcNow().Returns(now);
        var scheduler = Substitute.For<IScheduler>();
        var options = new FileUploadReconciliationOptions { DeletedFileRecheckWindow = TimeSpan.FromHours(2) };
        var cleanup = new FileContentCleanupScheduler(
            scheduler,
            Microsoft.Extensions.Options.Options.Create(options),
            clock);
        var key = FileStorageKey.Create("contents/unused").Value;

        await cleanup.ScheduleAsync(key, TestContext.Current.CancellationToken);

        await scheduler.Received(1).ScheduleAsync(
            Arg.Is<DeleteStoredFileCommand>(command =>
                command.StorageKey == key.Value && command.RecheckUntil == now.AddHours(2)),
            TimeSpan.Zero,
            TestContext.Current.CancellationToken);
    }

    [Theory(DisplayName = "Cleanup scheduler should repeat checks when the recheck window is open")]
    [InlineData(-1, true)]
    [InlineData(0, false)]
    [InlineData(1, false)]
    public async Task RescheduleAsync_Should_RepeatChecks_When_TheRecheckWindowIsOpen(
        int minutesAfterDeadline,
        bool scheduled)
    {
        var deadline = DateTimeOffset.UtcNow;
        var clock = Substitute.For<TimeProvider>();
        clock.GetUtcNow().Returns(deadline.AddMinutes(minutesAfterDeadline));
        var scheduler = Substitute.For<IScheduler>();
        var options = new FileUploadReconciliationOptions { MinimumAge = TimeSpan.FromMinutes(5) };
        var cleanup = new FileContentCleanupScheduler(
            scheduler,
            Microsoft.Extensions.Options.Options.Create(options),
            clock);
        var command = new DeleteStoredFileCommand("contents/unused", deadline);

        await cleanup.RescheduleAsync(command, TestContext.Current.CancellationToken);

        await scheduler.Received(scheduled ? 1 : 0).ScheduleAsync(
            Arg.Is<DeleteStoredFileCommand>(message => ReferenceEquals(message, command)),
            options.MinimumAge,
            TestContext.Current.CancellationToken);
    }
}
