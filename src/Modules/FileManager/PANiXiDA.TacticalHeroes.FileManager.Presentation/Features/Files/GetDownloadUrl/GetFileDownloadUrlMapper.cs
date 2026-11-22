using PANiXiDA.Core.Application.Storage.Models;

using PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Files.GetDownloadUrl;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Presentation.Features.Files.GetDownloadUrl;

[Mapper]
internal static partial class GetFileDownloadUrlMapper
{
    internal static partial GetFileDownloadUrlQuery ToQuery(Guid id);

    internal static partial GetFileDownloadUrlResponse ToResponse(PresignedDownloadUrl download);
}
