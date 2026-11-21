using System.Text.Json;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Write;

internal sealed class FileRelocationConverter() : ValueConverter<FileRelocation?, string>(
    relocation => Serialize(relocation!),
    value => Deserialize(value))
{
    private static string Serialize(FileRelocation relocation)
    {
        return JsonSerializer.Serialize(
            new
            {
                AttemptId = relocation.AttemptId.Value,
                Name = relocation.Name.Value,
                Type = relocation.Type.Name,
                UserId = relocation.UserId?.Value,
                FolderId = relocation.FolderId?.Value,
                StorageKey = relocation.StorageKey.Value
            });
    }

    private static FileRelocation Deserialize(string value)
    {
        using var document = JsonDocument.Parse(value);
        var root = document.RootElement;
        var attemptId = root.GetProperty(nameof(FileRelocation.AttemptId)).GetGuid();
        var owner = root.GetProperty(nameof(FileRelocation.UserId));
        var folder = root.GetProperty(nameof(FileRelocation.FolderId));

        return FileRelocation.Create(
            attemptId: FileRelocationAttemptId.Create(attemptId).Value,
            name: FileName.Create(root.GetProperty(nameof(FileRelocation.Name)).GetString()!).Value,
            type: FileType.Create(root.GetProperty(nameof(FileRelocation.Type)).GetString()!).Value,
            userId: UserId.Create(owner.ValueKind == JsonValueKind.Null ? null : owner.GetGuid()).Value,
            folderId: FolderId.Create(folder.ValueKind == JsonValueKind.Null ? null : folder.GetGuid()).Value,
            storageKey: FileStorageKey.Create(root.GetProperty(nameof(FileRelocation.StorageKey)).GetString()!).Value);
    }
}
