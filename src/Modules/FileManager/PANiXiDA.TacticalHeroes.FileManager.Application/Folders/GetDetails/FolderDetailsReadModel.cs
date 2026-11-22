namespace PANiXiDA.TacticalHeroes.FileManager.Application.Folders.GetDetails;

public sealed record FolderDetailsReadModel(
    Guid Id,
    string Name,
    string AllowedFileType,
    Guid? UserId,
    Guid? ParentId,
    string? ParentName) : IReadModel;
