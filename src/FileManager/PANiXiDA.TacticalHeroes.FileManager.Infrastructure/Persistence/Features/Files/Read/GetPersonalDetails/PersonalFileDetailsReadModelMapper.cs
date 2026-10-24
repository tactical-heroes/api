using PANiXiDA.TacticalHeroes.FileManager.Application.Files.GetPersonalDetails;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Read.DbModels;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Read.GetPersonalDetails;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal sealed partial class PersonalFileDetailsReadModelMapper
    : IReadModelMapper<Guid, FileReadDbModel, PersonalFileDetailsReadModel>
{
    public static partial IQueryable<PersonalFileDetailsReadModel> ProjectTo(
        IQueryable<FileReadDbModel> query);

    private static partial PersonalFileDetailsReadModel ToReadModel(FileReadDbModel file);
}
