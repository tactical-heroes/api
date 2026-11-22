namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.Update;

public sealed record UpdateFileRequest(
    string Name,
    string Type,
    Guid? UserId = null,
    Guid? FolderId = null);
