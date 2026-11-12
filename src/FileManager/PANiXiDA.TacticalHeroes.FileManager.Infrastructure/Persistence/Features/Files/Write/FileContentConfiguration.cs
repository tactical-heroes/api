using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PANiXiDA.TacticalHeroes.FileManager.Domain.Files;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Entities.FileContents;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Entities.FileContents.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Write;

internal sealed class FileContentConfiguration : IEntityTypeConfiguration<FileContent>
{
    public void Configure(EntityTypeBuilder<FileContent> builder)
    {
        builder.HasKey(content => content.Id);
        builder.Property(content => content.Id)
            .HasConversion(id => id.Value, value => FileContentId.Create(value).Value)
            .ValueGeneratedNever();
        builder.Property(content => content.FileId)
            .HasConversion(id => id.Value, value => FileId.Create(value).Value)
            .IsRequired();
        builder.Property(content => content.StorageKey)
            .HasConversion(key => key.Value, value => FileStorageKey.Create(value).Value)
            .HasMaxLength(FileStorageKey.MaxLength)
            .IsRequired();
        builder.Property(content => content.RegisteredAt)
            .HasConversion(time => time.Value, value => FileContentTimestamp.Create(value))
            .IsRequired();
        builder.Property(content => content.DeletionStartedAt)
            .HasConversion(time => time!.Value, value => FileContentTimestamp.Create(value))
            .IsRequired(required: false);
        builder.HasIndex(content => content.StorageKey).IsUnique();
        builder.HasIndex(content => content.FileId);
    }
}
