namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.Read.GetDetails;

public sealed record GetFileDetailsQuery(Guid Id)
    : IQuery<Result<FileDetailsReadModel>>;
