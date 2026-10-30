using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Events;

namespace PANiXiDA.TacticalHeroes.FileManager.UnitTests.Domain.Folders.Events;

public sealed class FolderDeletedTests
{
    [Fact(DisplayName = "Folder deleted event should preserve the folder id when constructed")]
    public void Constructor_Should_PreserveFolderId_When_Constructed()
    {
        var id = Guid.NewGuid();

        var domainEvent = new FolderDeleted(id);

        domainEvent.FolderId.ShouldBe(id);
        domainEvent.Id.ShouldNotBe(Guid.Empty);
    }
}
