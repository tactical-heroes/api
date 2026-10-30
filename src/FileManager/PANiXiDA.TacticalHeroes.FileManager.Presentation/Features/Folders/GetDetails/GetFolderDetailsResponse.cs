namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Folders.GetDetails;

public sealed record GetFolderDetailsResponse(
    Guid Id,
    string Name,
    string AllowedFileType,
    Guid? UserId,
    Guid? ParentId,
    string? ParentName);
