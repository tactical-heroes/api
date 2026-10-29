using PANiXiDA.TacticalHeroes.FileManager.Application.Files.GetDetails;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Read.DbModels;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Read.GetDetails;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal sealed partial class FileDetailsReadModelMapper
    : IReadModelMapper<Guid, FileReadDbModel, FileDetailsReadModel>
{
    public static partial IQueryable<FileDetailsReadModel> ProjectTo(
        IQueryable<FileReadDbModel> query);

    private static partial FileDetailsReadModel ToReadModel(FileReadDbModel file);
}
