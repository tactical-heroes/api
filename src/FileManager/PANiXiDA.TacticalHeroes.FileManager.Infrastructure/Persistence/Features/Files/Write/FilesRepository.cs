using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.FileManager.Domain.Files;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Write;

public sealed class FilesRepository(
    FileManagerWriteDbContext dbContext,
    IAggregateTracker aggregateTracker)
    : EfRepository<FileManagerWriteDbContext, FileId, File>(
        dbContext,
        aggregateTracker),
    IFilesRepository
{
    protected override IQueryable<File> Query => DbSet.AsTracking();
}
