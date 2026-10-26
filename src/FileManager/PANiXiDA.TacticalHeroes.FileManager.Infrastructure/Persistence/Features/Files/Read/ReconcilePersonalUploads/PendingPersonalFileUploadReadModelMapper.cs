using PANiXiDA.TacticalHeroes.FileManager.Application.Files.ReconcilePersonalUploads;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Read.DbModels;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Read.ReconcilePersonalUploads;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal sealed partial class PendingPersonalFileUploadReadModelMapper
    : IReadModelMapper<Guid, FileReadDbModel, PendingPersonalFileUploadReadModel>
{
    public static partial IQueryable<PendingPersonalFileUploadReadModel> ProjectTo(
        IQueryable<FileReadDbModel> query);

    private static partial PendingPersonalFileUploadReadModel ToReadModel(FileReadDbModel file);
}
