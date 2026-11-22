using PANiXiDA.Core.Application.Storage.Models;

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Files.GetDownloadUrl;

public sealed record GetFileDownloadUrlQuery(Guid Id)
    : IQuery<Result<PresignedDownloadUrl>>;
