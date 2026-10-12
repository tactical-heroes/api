namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.GetPersonalDetails;

public sealed record GetPersonalFileDetailsResponse(
    Guid Id,
    string Name,
    Guid? FolderId,
    string Status,
    string? ContentType,
    long? Size);
