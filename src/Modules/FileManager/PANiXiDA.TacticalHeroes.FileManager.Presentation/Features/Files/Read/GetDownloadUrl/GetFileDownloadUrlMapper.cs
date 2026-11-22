using PANiXiDA.Core.Application.Storage.Models;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Read.GetDownloadUrl;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.Read.GetDownloadUrl;

[Mapper]
internal static partial class GetFileDownloadUrlMapper
{
    internal static partial GetFileDownloadUrlQuery ToQuery(Guid id);

    internal static partial GetFileDownloadUrlResponse ToResponse(PresignedDownloadUrl download);
}
