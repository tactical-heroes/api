using PANiXiDA.TacticalHeroes.FileManager.Application.Files.GetList;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Read.DbModels;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Read.GetList;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal sealed partial class FileListItemReadModelMapper
    : IReadModelMapper<Guid, FileReadDbModel, FileListItemReadModel>
{
    public static partial IQueryable<FileListItemReadModel> ProjectTo(IQueryable<FileReadDbModel> query);

    private static partial FileListItemReadModel ToReadModel(FileReadDbModel file);
}
