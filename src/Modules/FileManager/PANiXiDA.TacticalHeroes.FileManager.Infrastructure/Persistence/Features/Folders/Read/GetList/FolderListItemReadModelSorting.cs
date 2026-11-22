using PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Folders.GetList;

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Infrastructure.Persistence.Features.Folders.Read.GetList;

internal sealed partial class FolderListItemReadModelSorting : IReadModelSorting<FolderListItemReadModel>
{
    public static SortingParameters DefaultSorting { get; } = SortingParameters.Of(
        new SortField(nameof(FolderListItemReadModel.Name)),
        new SortField(nameof(FolderListItemReadModel.Id), SortDirection.Desc));
}
