namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.Common.Filters;

public sealed record FilesFilter(string? Type = null, string? Search = null) : IFilter;
