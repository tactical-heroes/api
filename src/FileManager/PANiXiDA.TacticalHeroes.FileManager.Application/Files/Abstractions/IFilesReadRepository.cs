using PANiXiDA.TacticalHeroes.FileManager.Application.Files.GetPersonalDetails;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.ReconcilePersonalUploads;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.Abstractions;

public interface IFilesReadRepository : IReadRepository<Guid>
{
    Task<IReadOnlyList<PendingPersonalFileUploadReadModel>> GetPendingPersonalUploadsAsync(
        DateTime createdBefore,
        Guid afterId,
        int batchSize,
        CancellationToken cancellationToken);

    Task<PersonalFileDetailsReadModel?> GetPersonalDetailsByIdAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken);
}
