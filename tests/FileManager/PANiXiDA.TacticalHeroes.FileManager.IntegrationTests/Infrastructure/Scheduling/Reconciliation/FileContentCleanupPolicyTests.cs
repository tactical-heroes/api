using NSubstitute;

using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Entities.FileContents.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Scheduling.Options.FileUploadReconciliation;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Scheduling.Reconciliation;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.IntegrationTests.Infrastructure.Scheduling.Reconciliation;

public sealed class FileContentCleanupPolicyTests
{
    [Theory(DisplayName = "Content cleanup should honor configured ages when lifecycle varies")]
    [InlineData(5, false, false, false)]
    [InlineData(16, false, true, false)]
    [InlineData(5, true, true, false)]
    [InlineData(1500, true, true, true)]
    public void CanBeginDeletion_Should_HonorConfiguredAges_When_LifecycleVaries(
        int minutes, bool deleting, bool canDelete, bool canForget)
    {
        var now = DateTimeOffset.UtcNow;
        var then = FileContentTimestamp.Create(now.AddMinutes(-minutes));
        var file = File.Create(FileName.Create("file.txt").Value, FileType.Avatar, userId: null).Value;
        file.CompleteUpload(FileContentType.Create("text/plain").Value, FileSize.Create(4).Value);
        var content = file.BeginContentUpload(then).Value;
        if (deleting)
        {
            file.BeginContentDeletion(content.Id, then);
        }
        var clock = Substitute.For<TimeProvider>();
        clock.GetUtcNow().Returns(now);
        var options = Microsoft.Extensions.Options.Options.Create(new FileUploadReconciliationOptions());
        var policy = new FileContentCleanupPolicy(options, clock);

        var shouldDelete = policy.CanBeginDeletion(content);
        var shouldForget = policy.CanForget(content);

        shouldDelete.ShouldBe(canDelete);
        shouldForget.ShouldBe(canForget);
    }

    [Fact(DisplayName = "Content cleanup should retain pending uploads when recheck window has elapsed")]
    public void CanForget_Should_RetainPendingUploads_When_RecheckWindowHasElapsed()
    {
        var now = DateTimeOffset.UtcNow;
        var then = FileContentTimestamp.Create(now.AddDays(-2));
        var file = File.Create(FileName.Create("file.txt").Value, FileType.Avatar, userId: null).Value;
        file.CompleteUpload(FileContentType.Create("text/plain").Value, FileSize.Create(4).Value);
        var content = file.BeginContentUpload(then).Value;
        var clock = Substitute.For<TimeProvider>();
        clock.GetUtcNow().Returns(now);
        var options = Microsoft.Extensions.Options.Options.Create(new FileUploadReconciliationOptions());
        var policy = new FileContentCleanupPolicy(options, clock);

        var shouldForget = policy.CanForget(content);

        shouldForget.ShouldBeFalse();
    }
}
