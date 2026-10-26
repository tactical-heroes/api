using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.GetPersonalDetails;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.ReconcilePersonalUploads;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Read.ReconcilePersonalUploads;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Read.DbModels;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Read.GetPersonalDetails;

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Read;

public sealed class FilesReadRepository(FileManagerReadDbContext dbContext)
    :
        EfReadRepository<FileManagerReadDbContext, Guid, FileReadDbModel>(dbContext),
        IFilesReadRepository
{
    public async Task<IReadOnlyList<PendingPersonalFileUploadReadModel>> GetPendingPersonalUploadsAsync(
        DateTime createdBefore,
        Guid afterId,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var query = Query.Where(file =>
            file.Type == FileType.Personal.Name
                && file.Status == FileStatus.PendingUpload.Name
                && file.CreatedAt < createdBefore
                && file.Id.CompareTo(afterId) > 0);
        var pending = PendingPersonalFileUploadReadModelMapper.ProjectTo(query);

        return await PendingPersonalFileUploadReadModelSorting.ApplySorting(pending, SortingParameters.None)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public Task<PersonalFileDetailsReadModel?> GetPersonalDetailsByIdAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var query = Query.Where(file =>
            file.Id == id
                && file.Type == FileType.Personal.Name
                && file.UserId == userId);

        return PersonalFileDetailsReadModelMapper.ProjectTo(query)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
