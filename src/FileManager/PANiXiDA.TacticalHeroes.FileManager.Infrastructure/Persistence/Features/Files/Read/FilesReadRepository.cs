using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.GetPersonalDetails;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.GetPersonalDownloadUrl;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Read.DbModels;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Read.GetPersonalDetails;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Read.GetPersonalDownloadUrl;

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Read;

public sealed class FilesReadRepository(FileManagerReadDbContext dbContext)
    :
        EfReadRepository<FileManagerReadDbContext, Guid, FileReadDbModel>(dbContext),
        IFilesReadRepository
{
    public Task<PersonalFileDownloadReadModel?> GetPersonalDownloadByIdAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var query = Query.Where(file =>
            file.Id == id
                && file.Type == FileType.Personal.Name
                && file.UserId == userId);

        return PersonalFileDownloadReadModelMapper.ProjectTo(query)
            .FirstOrDefaultAsync(cancellationToken);
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
