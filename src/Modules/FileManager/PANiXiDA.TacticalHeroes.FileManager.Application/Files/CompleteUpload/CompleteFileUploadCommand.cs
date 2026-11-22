namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.CompleteUpload;

public sealed record CompleteFileUploadCommand(Guid Id) : ICommand<Result>;
