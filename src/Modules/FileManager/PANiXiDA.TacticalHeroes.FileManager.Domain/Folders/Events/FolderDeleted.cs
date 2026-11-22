namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Domain.Folders.Events;

public sealed record FolderDeleted(Guid FolderId) : DomainEvent;
