namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.GetPersonalDownloadUrl;

public sealed record GetPersonalFileDownloadUrlQuery(Guid Id)
    : IQuery<Result<PersonalFileDownloadUrlReadModel>>;
