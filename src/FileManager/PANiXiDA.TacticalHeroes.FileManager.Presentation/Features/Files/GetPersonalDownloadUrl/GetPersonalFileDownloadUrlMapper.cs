using PANiXiDA.TacticalHeroes.FileManager.Application.Files.GetPersonalDownloadUrl;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.GetPersonalDownloadUrl;

[Mapper]
internal static partial class GetPersonalFileDownloadUrlMapper
{
    internal static partial GetPersonalFileDownloadUrlQuery ToQuery(Guid id);

    internal static partial GetPersonalFileDownloadUrlResponse ToResponse(PersonalFileDownloadUrlReadModel download);
}
