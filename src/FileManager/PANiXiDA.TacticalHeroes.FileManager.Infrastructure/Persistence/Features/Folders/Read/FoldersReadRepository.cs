using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Folders.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Folders.Read;

public sealed class FoldersReadRepository(FileManagerReadDbContext dbContext)
    : EfReadRepository<FileManagerReadDbContext, Guid, FolderReadDbModel>(dbContext),
    IFoldersReadRepository;
