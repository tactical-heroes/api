using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.GetList;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Folders.Read.DbModels;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Folders.Read.GetList;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal sealed partial class FolderListItemReadModelMapper
    : IReadModelMapper<Guid, FolderReadDbModel, FolderListItemReadModel>
{
    public static partial IQueryable<FolderListItemReadModel> ProjectTo(IQueryable<FolderReadDbModel> query);

    [MapProperty(
        [nameof(FolderReadDbModel.Parent), nameof(FolderReadDbModel.Name)],
        nameof(FolderListItemReadModel.ParentName))]
    private static partial FolderListItemReadModel ToReadModel(FolderReadDbModel folder);
}
