using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Entities.FileContents.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;

namespace PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Entities.FileContents;

public sealed class FileContent : Entity<FileContentId>
{
    private FileContent(FileContentId id, FileId fileId, FileStorageKey storageKey, FileContentTimestamp registeredAt)
        : base(id)
    {
        FileId = fileId;
        StorageKey = storageKey;
        RegisteredAt = registeredAt;
    }

    public FileId FileId { get; private set; }
    public FileStorageKey StorageKey { get; private set; }
    public FileContentTimestamp RegisteredAt { get; private set; }
    public FileContentTimestamp? DeletionStartedAt { get; private set; }

    internal static FileContent Create(FileId fileId, FileStorageKey storageKey, FileContentTimestamp registeredAt)
    {
        return new FileContent(
            id: FileContentId.New(),
            fileId: fileId,
            storageKey: storageKey,
            registeredAt: registeredAt);
    }

    internal void BeginDeletion(FileContentTimestamp now)
    {
        DeletionStartedAt ??= now;
    }
}
