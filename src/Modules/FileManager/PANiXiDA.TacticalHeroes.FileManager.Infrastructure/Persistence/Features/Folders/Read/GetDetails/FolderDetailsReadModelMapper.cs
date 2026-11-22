using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.GetDetails;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Folders.Read.DbModels;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Folders.Read.GetDetails;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal sealed partial class FolderDetailsReadModelMapper
    : IReadModelMapper<Guid, FolderReadDbModel, FolderDetailsReadModel>
{
    public static partial IQueryable<FolderDetailsReadModel> ProjectTo(IQueryable<FolderReadDbModel> query);

    [MapProperty(
        [nameof(FolderReadDbModel.Parent), nameof(FolderReadDbModel.Name)],
        nameof(FolderDetailsReadModel.ParentName))]
    private static partial FolderDetailsReadModel ToReadModel(FolderReadDbModel folder);
}
