using PANiXiDA.TacticalHeroes.FileManager.Application.Files.GetDownloadUrl;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.GetDownloadUrl;

[Mapper]
internal static partial class GetFileDownloadUrlMapper
{
    internal static partial GetFileDownloadUrlQuery ToQuery(Guid id);

    internal static partial GetFileDownloadUrlResponse ToResponse(FileDownloadUrlReadModel download);
}
