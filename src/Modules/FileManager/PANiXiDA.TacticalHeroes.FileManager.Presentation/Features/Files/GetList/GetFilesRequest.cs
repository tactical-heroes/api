namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Presentation.Features.Files.GetList;

public sealed record GetFilesRequest(string? Type = null, string? Search = null, Guid? UserId = null);
