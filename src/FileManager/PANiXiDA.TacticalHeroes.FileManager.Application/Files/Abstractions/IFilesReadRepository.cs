using PANiXiDA.TacticalHeroes.FileManager.Application.Files.GetPersonalDetails;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.GetPersonalDownloadUrl;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.Abstractions;

public interface IFilesReadRepository : IReadRepository<Guid>
{
    Task<PersonalFileDownloadReadModel?> GetPersonalDownloadByIdAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken);

    Task<PersonalFileDetailsReadModel?> GetPersonalDetailsByIdAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken);
}
