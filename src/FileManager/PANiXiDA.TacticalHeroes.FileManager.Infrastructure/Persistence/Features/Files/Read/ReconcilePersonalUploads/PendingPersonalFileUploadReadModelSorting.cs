using PANiXiDA.TacticalHeroes.FileManager.Application.Files.ReconcilePersonalUploads;

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Read.ReconcilePersonalUploads;

internal sealed partial class PendingPersonalFileUploadReadModelSorting
    : IReadModelSorting<PendingPersonalFileUploadReadModel>
{
    public static SortingParameters DefaultSorting { get; } =
        SortingParameters.Of(new SortField(nameof(PendingPersonalFileUploadReadModel.Id), SortDirection.Asc));
}
