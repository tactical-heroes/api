namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Files.GetDetails;

public sealed record GetFileDetailsQuery(Guid Id)
    : IQuery<Result<FileDetailsReadModel>>;
