using PANiXiDA.Core.Application.Storage.Models;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.Read.GetDownloadUrl;

public sealed record GetFileDownloadUrlQuery(Guid Id)
    : IQuery<Result<PresignedDownloadUrl>>;
