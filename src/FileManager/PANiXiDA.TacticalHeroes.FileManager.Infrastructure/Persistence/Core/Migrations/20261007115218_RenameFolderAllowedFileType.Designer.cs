using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;

#nullable disable

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core.Migrations
{
    [DbContext(typeof(FileManagerWriteDbContext))]
    [Migration("20261007115218_RenameFolderAllowedFileType")]
    partial class RenameFolderAllowedFileType
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("file_manager")
                .HasAnnotation("ProductVersion", "10.0.12")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseHiLo(modelBuilder, "EntityFrameworkHiLoSequence");

            modelBuilder.HasSequence("EntityFrameworkHiLoSequence")
                .IncrementsBy(10);

            modelBuilder.Entity("PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("ContentType")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("content_type");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at")
                        .HasColumnOrder(1);

                    b.Property<DateTime?>("DeletedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("deleted_at")
                        .HasColumnOrder(3);

                    b.Property<Guid?>("FolderId")
                        .HasColumnType("uuid")
                        .HasColumnName("folder_id");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("name");

                    b.Property<long?>("Size")
                        .HasColumnType("bigint")
                        .HasColumnName("size");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)")
                        .HasColumnName("status");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)")
                        .HasColumnName("type");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_at")
                        .HasColumnOrder(2);

                    b.Property<uint>("Version")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("xid")
                        .HasColumnName("xmin");

                    b.HasKey("Id")
                        .HasName("pk_files");

                    b.HasIndex("FolderId")
                        .HasDatabaseName("ix_files_folder_id");

                    b.HasIndex("Status")
                        .HasDatabaseName("ix_files_status");

                    b.ToTable("files", "file_manager");
                });

            modelBuilder.Entity("PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Folder", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("AllowedFileType")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)")
                        .HasColumnName("allowed_file_type");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at")
                        .HasColumnOrder(1);

                    b.Property<DateTime?>("DeletedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("deleted_at")
                        .HasColumnOrder(3);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("name");

                    b.Property<Guid?>("ParentId")
                        .HasColumnType("uuid")
                        .HasColumnName("parent_id");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_at")
                        .HasColumnOrder(2);

                    b.HasKey("Id")
                        .HasName("pk_folders");

                    b.HasIndex("ParentId")
                        .HasDatabaseName("ix_folders_parent_id");

                    b.ToTable("folders", "file_manager");
                });

            modelBuilder.Entity("PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File", b =>
                {
                    b.HasOne("PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Folder", null)
                        .WithMany()
                        .HasForeignKey("FolderId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .HasConstraintName("fk_files_folder_folder_id");
                });

            modelBuilder.Entity("PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Folder", b =>
                {
                    b.HasOne("PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Folder", null)
                        .WithMany()
                        .HasForeignKey("ParentId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .HasConstraintName("fk_folders_folders_parent_id");
                });
#pragma warning restore 612, 618
        }
    }
}
