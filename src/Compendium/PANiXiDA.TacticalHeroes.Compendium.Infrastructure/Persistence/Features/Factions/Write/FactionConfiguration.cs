using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Factions.Write;

internal sealed class FactionConfiguration : AuditableEntityConfiguration<Faction>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Faction> builder)
    {
        builder.HasKey(faction => faction.Id);

        builder.Property(faction => faction.Id)
            .HasConversion(
                id => id.Value,
                value => new FactionId(value))
            .ValueGeneratedNever();

        builder.Property(faction => faction.Name)
            .HasConversion(
                name => name.Value,
                value => FactionName.Create(value).Value)
            .HasMaxLength(FactionName.MaxLength)
            .IsRequired();

        builder.Property(faction => faction.Description)
            .HasConversion(
                description => description.Value,
                value => FactionDescription.Create(value).Value)
            .HasMaxLength(FactionDescription.MaxLength)
            .IsRequired();
    }
}
