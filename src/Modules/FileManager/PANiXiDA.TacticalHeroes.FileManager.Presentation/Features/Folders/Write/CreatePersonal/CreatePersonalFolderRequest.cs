namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Folders.Write.CreatePersonal;

public sealed record CreatePersonalFolderRequest(
    string Name,
    Guid? ParentId = null);
