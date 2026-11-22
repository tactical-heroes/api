namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Folders.GetList;

public sealed record GetFoldersRequest(string? Search = null, string? AllowedFileType = null, Guid? UserId = null);
