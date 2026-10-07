using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;

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

        builder.Property(file => file.Type)
            .HasConversion(
                type => type.Name,
                value => FileType.Create(value).Value)
            .HasMaxLength(FileType.MaxLength)
            .IsRequired();

        builder.Property(file => file.Status)
            .HasConversion(
                status => status.Name,
                value => FileStatus.Create(value).Value)
            .HasMaxLength(FileStatus.MaxLength)
            .IsRequired();

        builder.Property(file => file.ContentType)
            .HasConversion(
                contentType => contentType!.Value,
                value => FileContentType.Create(value).Value)
            .HasMaxLength(FileContentType.MaxLength)
            .IsRequired(required: false);

        builder.Property(file => file.Size)
            .HasConversion(
                size => size!.Value,
                value => FileSize.Create(value).Value)
            .IsRequired(required: false);

        builder.Property(file => file.FolderId)
            .HasConversion(
                id => id!.Value.Value,
                value => FolderId.Create(value).Value)
            .IsRequired(required: false);

        builder.Property<uint>("Version")
            .IsRowVersion();

        builder.HasIndex(file => file.Status);
        builder.HasIndex(file => file.FolderId);

        builder.HasOne<Folder>()
            .WithMany()
            .HasForeignKey(file => file.FolderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
