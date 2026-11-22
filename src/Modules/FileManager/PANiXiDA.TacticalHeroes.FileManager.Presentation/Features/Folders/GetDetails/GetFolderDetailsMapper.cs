using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.GetDetails;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Folders.GetDetails;

[Mapper]
internal static partial class GetFolderDetailsMapper
{
    internal static partial GetFolderDetailsQuery ToQuery(Guid id);

    internal static partial GetFolderDetailsResponse ToResponse(FolderDetailsReadModel folder);
}
