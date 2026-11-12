using PANiXiDA.TacticalHeroes.FileManager.Domain.Files;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Entities.FileContents;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Entities.FileContents.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Domain.Files.Entities.FileContents;

public sealed class FileContentTests
{
    [Fact(DisplayName = "File content should retain its address when created")]
    public void Create_Should_RetainAddress_When_Created()
    {
        var fileId = FileId.New();
        var key = FileStorageKey.Create("contents/example").Value;
        var now = FileContentTimestamp.Create(DateTimeOffset.UtcNow);

        var content = FileContent.Create(fileId, key, now);

        content.Id.Value.Version.ShouldBe(7);
        content.FileId.ShouldBe(fileId);
        content.StorageKey.ShouldBeSameAs(key);
        content.RegisteredAt.ShouldBe(now);
        content.DeletionStartedAt.ShouldBeNull();
    }

    [Fact(DisplayName = "File content should retain deletion time when deletion is retried")]
    public void BeginDeletion_Should_RetainDeletionTime_When_DeletionIsRetried()
    {
        var now = FileContentTimestamp.Create(DateTimeOffset.UtcNow);
        var content = FileContent.Create(FileId.New(), FileStorageKey.Create("contents/example").Value, now);

        content.BeginDeletion(now);
        content.BeginDeletion(FileContentTimestamp.Create(now.Value.AddMinutes(5)));

        content.DeletionStartedAt.ShouldBe(now);
    }
}
