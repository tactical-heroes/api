using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;

namespace PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;

public sealed partial class FileRelocation : ValueObject
{
    private FileRelocation(
        FileRelocationAttemptId attemptId,
        FileName name,
        FileType type,
        UserId? userId,
        FolderId? folderId,
        FileStorageKey storageKey)
    {
        AttemptId = attemptId;
        Name = name;
        Type = type;
        UserId = userId;
        FolderId = folderId;
        StorageKey = storageKey;
    }

    public FileRelocationAttemptId AttemptId { get; }
    public FileName Name { get; }
    public FileType Type { get; }
    public UserId? UserId { get; }
    public FolderId? FolderId { get; }
    public FileStorageKey StorageKey { get; }

    public static FileRelocation Create(
        FileRelocationAttemptId attemptId,
        FileName name,
        FileType type,
        UserId? userId,
        FolderId? folderId,
        FileStorageKey storageKey)
    {
        return new FileRelocation(
            attemptId: attemptId,
            name: name,
            type: type,
            userId: userId,
            folderId: folderId,
            storageKey: storageKey);
    }
}
