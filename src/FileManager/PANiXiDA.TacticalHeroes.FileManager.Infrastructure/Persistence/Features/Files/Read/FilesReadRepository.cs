using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Read;

public sealed class FilesReadRepository(FileManagerReadDbContext dbContext)
    : EfReadRepository<FileManagerReadDbContext, Guid, FileReadDbModel>(dbContext),
    IFilesReadRepository;
