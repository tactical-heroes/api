namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.Write.Update;

public sealed record UpdateFileRequest(
    string Name,
    string Type,
    Guid? UserId = null,
    Guid? FolderId = null);
