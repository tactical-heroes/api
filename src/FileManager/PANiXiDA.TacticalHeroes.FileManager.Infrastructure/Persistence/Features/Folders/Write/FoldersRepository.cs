using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Folders.Write;

public sealed class FoldersRepository(
    FileManagerWriteDbContext dbContext,
    IAggregateTracker aggregateTracker)
    : EfRepository<FileManagerWriteDbContext, FolderId, Folder>(
        dbContext,
        aggregateTracker),
    IFoldersRepository;
