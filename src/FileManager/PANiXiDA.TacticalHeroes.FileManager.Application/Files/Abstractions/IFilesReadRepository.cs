using PANiXiDA.TacticalHeroes.FileManager.Application.Files.GetPersonalDetails;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.Abstractions;

public interface IFilesReadRepository : IReadRepository<Guid>
{
    Task<PersonalFileDetailsReadModel?> GetPersonalDetailsByIdAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken);
}
