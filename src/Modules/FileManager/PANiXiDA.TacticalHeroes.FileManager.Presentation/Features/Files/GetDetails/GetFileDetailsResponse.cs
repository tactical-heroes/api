namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.GetDetails;

public sealed record GetFileDetailsResponse(
    Guid Id,
    string Name,
    string Type,
    Guid? FolderId,
    string Status,
    string? ContentType,
    long? Size);
