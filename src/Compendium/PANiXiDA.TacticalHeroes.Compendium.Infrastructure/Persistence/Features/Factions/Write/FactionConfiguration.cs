using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Factions.Write;

internal sealed class FactionConfiguration : AuditableEntityConfiguration<Faction>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Faction> builder)
    {
        builder.HasKey(keyExpression: faction => faction.Id);

        builder.Property(propertyExpression: faction => faction.Id)
            .HasConversion(
                convertToProviderExpression: id => id.Value,
                convertFromProviderExpression: value => new FactionId(Value: value))
            .ValueGeneratedNever();

        builder.Property(propertyExpression: faction => faction.Name)
            .HasConversion(
                convertToProviderExpression: name => name.Value,
                convertFromProviderExpression: value => FactionName.Create(value: value).Value)
            .HasMaxLength(maxLength: FactionName.MaxLength)
            .IsRequired();

        builder.Property(propertyExpression: faction => faction.Description)
            .HasConversion(
                convertToProviderExpression: description => description.Value,
                convertFromProviderExpression: value => FactionDescription.Create(value: value).Value)
            .HasMaxLength(maxLength: FactionDescription.MaxLength)
            .IsRequired();
    }
}
