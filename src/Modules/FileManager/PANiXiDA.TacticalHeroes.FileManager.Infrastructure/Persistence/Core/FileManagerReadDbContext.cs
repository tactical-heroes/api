using Microsoft.EntityFrameworkCore;

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;

public sealed class FileManagerReadDbContext(
    DbContextOptions<FileManagerReadDbContext> options)
    : ReadDbContext<FileManagerReadDbContext>(options)
{
    protected override bool UseContextNameAsSchema => true;
}
