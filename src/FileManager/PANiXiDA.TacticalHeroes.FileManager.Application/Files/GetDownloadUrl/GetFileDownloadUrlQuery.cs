namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.GetDownloadUrl;

public sealed record GetFileDownloadUrlQuery(Guid Id)
    : IQuery<Result<FileDownloadUrlReadModel>>;
