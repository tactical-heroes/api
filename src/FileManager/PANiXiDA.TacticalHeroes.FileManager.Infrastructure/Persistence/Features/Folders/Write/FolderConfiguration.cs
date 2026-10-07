using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Users;

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Folders.Write;

internal sealed class FolderConfiguration : AuditableEntityConfiguration<Folder>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Folder> builder)
    {
        builder.HasKey(folder => folder.Id);

        builder.Property(folder => folder.Id)
            .HasConversion(
                id => id.Value,
                value => FolderId.Create(value).Value)
            .ValueGeneratedNever();

        builder.Property(folder => folder.Name)
            .HasConversion(
                name => name.Value,
                value => FolderName.Create(value).Value)
            .HasMaxLength(FolderName.MaxLength)
            .IsRequired();

        builder.Property(folder => folder.AllowedFileType)
            .HasConversion(
                type => type.Name,
                value => FileType.Create(value).Value)
            .HasMaxLength(FileType.MaxLength)
            .IsRequired();

        builder.Property(folder => folder.ParentId)
            .HasConversion(
                id => id!.Value.Value,
                value => FolderId.Create(value).Value)
            .IsRequired(required: false);

        builder.Property(folder => folder.UserId)
            .HasConversion(
                id => id!.Value.Value,
                value => UserId.Create(value).Value)
            .IsRequired(required: false);

        builder.Property<uint>("Version")
            .IsRowVersion();

        builder.HasIndex(folder => folder.ParentId);
        builder.HasIndex(folder => folder.UserId);

        builder.HasOne<Folder>()
            .WithMany()
            .HasForeignKey(folder => folder.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
