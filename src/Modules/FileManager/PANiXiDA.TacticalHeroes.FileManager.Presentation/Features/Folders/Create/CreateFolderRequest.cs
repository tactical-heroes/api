namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Presentation.Features.Folders.Create;

public sealed record CreateFolderRequest(
    string Name,
    string AllowedFileType,
    Guid? UserId = null,
    Guid? ParentId = null);
