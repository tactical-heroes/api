namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Files.Common.Filters;

public sealed record FilesFilter(string? Type = null, string? Search = null, Guid? UserId = null) : IFilter;
