using System;
using System.Collections.Generic;

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
    [Migration("20261007063900_AddFiles")]
    partial class AddFiles
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("file_manager")
                .HasAnnotation(
                    "ProductVersion",
                    "10.0.12")
                .HasAnnotation(
                    "Relational:MaxIdentifierLength",
                    63);

            NpgsqlModelBuilderExtensions.UseHiLo(
                modelBuilder,
                "EntityFrameworkHiLoSequence");

            modelBuilder.HasSequence("EntityFrameworkHiLoSequence")
                .IncrementsBy(10);

            modelBuilder.Entity(
                "PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File",
                b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("id");

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

                    b.Property<string>("Purpose")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)")
                        .HasColumnName("purpose");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)")
                        .HasColumnName("status");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_at")
                        .HasColumnOrder(2);

                    b.Property<uint>("Version")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("xid")
                        .HasColumnName("xmin");

                    b.ComplexProperty(
                        typeof(Dictionary<string, object>),
                        "Content",
                        "PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File.Content#FileContent",
                        b1 =>
                        {
                            b1.Property<string>("ContentType")
                                .IsRequired()
                                .HasMaxLength(255)
                                .HasColumnType("character varying(255)")
                                .HasColumnName("content_content_type");

                            b1.Property<string>("Sha256")
                                .IsRequired()
                                .HasMaxLength(64)
                                .HasColumnType("character varying(64)")
                                .HasColumnName("content_sha256");

                            b1.Property<long>("Size")
                                .HasColumnType("bigint")
                                .HasColumnName("content_size");
                        });

                    b.HasKey("Id")
                        .HasName("pk_files");

                    b.HasIndex("Status")
                        .HasDatabaseName("ix_files_status");

                    b.ToTable(
                        "files",
                        "file_manager");
                });
#pragma warning restore 612, 618
        }
    }
}
