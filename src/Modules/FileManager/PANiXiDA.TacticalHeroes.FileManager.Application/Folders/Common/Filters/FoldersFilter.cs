namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Folders.Common.Filters;

public sealed record FoldersFilter(
    string? Search = null,
    string? AllowedFileType = null,
    Guid? UserId = null) : IFilter;
