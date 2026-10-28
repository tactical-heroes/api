namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.Common.Filters;

public sealed record FilesFilter(string Type, string? Search = null) : IFilter;
