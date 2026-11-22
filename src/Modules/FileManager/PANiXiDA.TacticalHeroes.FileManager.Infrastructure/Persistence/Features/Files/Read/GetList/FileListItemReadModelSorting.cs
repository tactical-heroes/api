using PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Files.GetList;

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Infrastructure.Persistence.Features.Files.Read.GetList;

internal sealed partial class FileListItemReadModelSorting : IReadModelSorting<FileListItemReadModel>
{
    public static SortingParameters DefaultSorting { get; } = SortingParameters.Of(
        new SortField(nameof(FileListItemReadModel.Name)),
        new SortField(nameof(FileListItemReadModel.Id), SortDirection.Desc));
}
