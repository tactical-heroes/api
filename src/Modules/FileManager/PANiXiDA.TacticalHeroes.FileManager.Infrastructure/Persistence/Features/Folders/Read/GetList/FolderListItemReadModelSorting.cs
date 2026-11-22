using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.GetList;

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Folders.Read.GetList;

internal sealed partial class FolderListItemReadModelSorting : IReadModelSorting<FolderListItemReadModel>
{
    public static SortingParameters DefaultSorting { get; } = SortingParameters.Of(
        new SortField(nameof(FolderListItemReadModel.Name)),
        new SortField(nameof(FolderListItemReadModel.Id), SortDirection.Desc));
}
