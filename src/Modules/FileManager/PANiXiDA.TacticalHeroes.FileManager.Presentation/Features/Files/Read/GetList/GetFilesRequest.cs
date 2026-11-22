namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.Read.GetList;

public sealed record GetFilesRequest(string? Type = null, string? Search = null, Guid? UserId = null);
