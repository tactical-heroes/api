using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Read.GetDetails;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Folders.Read.GetDetails;

[Mapper]
internal static partial class GetFolderDetailsMapper
{
    internal static partial GetFolderDetailsQuery ToQuery(Guid id);

    internal static partial GetFolderDetailsResponse ToResponse(FolderDetailsReadModel folder);
}
