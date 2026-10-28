namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.Policies;

public sealed record FileReadAccess(
    bool CanReadAllPersonalFiles,
    Guid? PersonalFilesUserId,
    bool CanReadAvatars);
