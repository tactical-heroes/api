namespace PANiXiDA.TacticalHeroes.FileManager.Application.Folders.GetList;

public sealed record FolderListItemReadModel(Guid Id, string Name, string AllowedFileType, Guid? ParentId) : IReadModel;
