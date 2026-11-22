namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Presentation.Features.Folders.CreatePersonal;

public sealed record CreatePersonalFolderRequest(
    string Name,
    Guid? ParentId = null);
