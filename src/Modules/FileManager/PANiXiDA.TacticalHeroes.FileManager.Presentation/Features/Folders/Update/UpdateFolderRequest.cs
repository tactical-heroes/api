namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Folders.Update;

public sealed record UpdateFolderRequest(
    string Name,
    string AllowedFileType,
    Guid? UserId = null,
    Guid? ParentId = null);
