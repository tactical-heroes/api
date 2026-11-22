namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.Read.GetDetails;

public sealed record GetFileDetailsResponse(
    Guid Id,
    string Name,
    string Type,
    Guid? FolderId,
    string Status,
    string? ContentType,
    long? Size);
