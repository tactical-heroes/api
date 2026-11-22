using PANiXiDA.TacticalHeroes.FileManager.Application.Files.GetList;

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Read.GetList;

internal sealed partial class FileListItemReadModelSorting : IReadModelSorting<FileListItemReadModel>
{
    public static SortingParameters DefaultSorting { get; } = SortingParameters.Of(
        new SortField(nameof(FileListItemReadModel.Name)),
        new SortField(nameof(FileListItemReadModel.Id), SortDirection.Desc));
}
