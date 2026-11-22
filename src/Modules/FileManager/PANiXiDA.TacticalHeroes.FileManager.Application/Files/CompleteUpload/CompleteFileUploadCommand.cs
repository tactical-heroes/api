namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Files.CompleteUpload;

public sealed record CompleteFileUploadCommand(Guid Id) : ICommand<Result>;
