using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;

public sealed class FileManagerReadDbContext(
    DbContextOptions<FileManagerReadDbContext> options)
    : ReadDbContext<FileManagerReadDbContext>(options)
{
    protected override bool UseContextNameAsSchema => true;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<FileReadDbModel>()
            .Property(file => file.Version)
            .IsRowVersion();
    }
}
