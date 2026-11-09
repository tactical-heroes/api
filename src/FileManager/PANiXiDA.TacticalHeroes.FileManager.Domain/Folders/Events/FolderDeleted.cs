namespace PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Events;

public sealed record FolderDeleted(Guid FolderId) : DomainEvent;
