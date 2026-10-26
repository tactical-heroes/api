namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.ReconcilePersonalUploads;

public sealed record PendingPersonalFileUploadReadModel(Guid Id, string StorageKey) : IReadModel;
