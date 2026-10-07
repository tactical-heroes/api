using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PANiXiDA.TacticalHeroes.FileManager.Domain.Files;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Write;

internal sealed class FileConfiguration : AuditableEntityConfiguration<File>
{
    protected override void ConfigureEntity(EntityTypeBuilder<File> builder)
    {
        builder.HasKey(file => file.Id);

        builder.Property(file => file.Id)
            .HasConversion(
                id => id.Value,
                value => FileId.Create(value).Value)
            .ValueGeneratedNever();

        builder.Property(file => file.Name)
            .HasConversion(
                name => name.Value,
                value => FileName.Create(value).Value)
            .HasMaxLength(FileName.MaxLength)
            .IsRequired();

        builder.Property(file => file.Purpose)
            .HasConversion(
                purpose => purpose.Name,
                value => FilePurpose.Create(value).Value)
            .HasMaxLength(FilePurpose.MaxLength)
            .IsRequired();

        builder.Property(file => file.Status)
            .HasConversion(
                status => status.Name,
                value => FileStatus.Create(value).Value)
            .HasMaxLength(FileStatus.MaxLength)
            .IsRequired();

        builder.ComplexProperty(file => file.Content, content =>
        {
            content.IsRequired(required: false);

            content.Property(value => value.ContentType)
                .HasConversion(
                    contentType => contentType.Value,
                    value => FileContentType.Create(value).Value)
                .HasMaxLength(FileContentType.MaxLength)
                .IsRequired();

            content.Property(value => value.Size)
                .IsRequired();

            content.Property(value => value.Sha256)
                .HasConversion(
                    checksum => checksum.Value,
                    value => FileChecksum.Create(value).Value)
                .HasMaxLength(FileChecksum.MaxLength)
                .IsRequired();
        });

        builder.Property<uint>("Version")
            .IsRowVersion();

        builder.HasIndex(file => file.Status);
    }
}
