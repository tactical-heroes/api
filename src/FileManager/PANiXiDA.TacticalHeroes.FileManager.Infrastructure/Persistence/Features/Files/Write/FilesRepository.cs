using PANiXiDA.TacticalHeroes.FileManager.Domain.Files;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Write;

public sealed class FilesRepository(
    FileManagerWriteDbContext dbContext)
    :
        EfRepository<FileManagerWriteDbContext, FileId, File>(
            dbContext),
        IFilesRepository;
