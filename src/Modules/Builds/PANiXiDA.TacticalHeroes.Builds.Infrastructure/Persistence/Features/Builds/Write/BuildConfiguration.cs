using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PANiXiDA.TacticalHeroes.Builds.Domain.Builds;
using PANiXiDA.TacticalHeroes.Builds.Domain.Builds.ValueObjects;
using PANiXiDA.TacticalHeroes.Builds.Domain.Players;

namespace PANiXiDA.TacticalHeroes.Builds.Infrastructure.Persistence.Features.Builds.Write;

internal sealed class BuildConfiguration : AuditableEntityConfiguration<Build>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Build> builder)
    {
        builder.HasKey(build => build.Id);
        builder.Property(build => build.Id)
            .HasConversion(id => id.Value, value => BuildId.Create(value).Value)
            .ValueGeneratedNever();
        builder.Property(build => build.PlayerId)
            .HasConversion(id => id.Value, value => PlayerId.Create(value).Value)
            .IsRequired();
        builder.Property(build => build.Name)
            .HasConversion(name => name.Value, value => BuildName.Create(value).Value)
            .HasMaxLength(BuildName.MaxLength)
            .IsRequired();

        builder.HasIndex(build => build.PlayerId);

        builder.HasMany(build => build.Heroes)
            .WithOne()
            .HasForeignKey(hero => hero.BuildId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(build => build.Heroes)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();

        builder.HasMany(build => build.Units)
            .WithOne()
            .HasForeignKey(unit => unit.BuildId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(build => build.Units)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();
    }
}
