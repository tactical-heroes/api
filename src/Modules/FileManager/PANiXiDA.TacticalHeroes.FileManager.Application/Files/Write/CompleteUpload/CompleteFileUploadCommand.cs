namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.Write.CompleteUpload;

public sealed record CompleteFileUploadCommand(Guid Id) : ICommand<Result>;
