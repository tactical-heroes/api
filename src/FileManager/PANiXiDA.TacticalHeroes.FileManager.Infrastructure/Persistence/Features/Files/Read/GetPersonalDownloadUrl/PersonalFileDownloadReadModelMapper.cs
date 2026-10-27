using PANiXiDA.TacticalHeroes.FileManager.Application.Files.GetPersonalDownloadUrl;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Read.DbModels;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Read.GetPersonalDownloadUrl;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal sealed partial class PersonalFileDownloadReadModelMapper
    : IReadModelMapper<Guid, FileReadDbModel, PersonalFileDownloadReadModel>
{
    public static partial IQueryable<PersonalFileDownloadReadModel> ProjectTo(
        IQueryable<FileReadDbModel> query);

    private static partial PersonalFileDownloadReadModel ToReadModel(FileReadDbModel file);
}
