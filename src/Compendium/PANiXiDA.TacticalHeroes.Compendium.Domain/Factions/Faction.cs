using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;

public sealed class Faction : AggregateRoot<FactionId>
{
    private Faction(
        FactionId id,
        FactionName name,
        FactionDescription description)
        : base(id)
    {
        Name = name;
        Description = description;
    }

    public FactionName Name { get; private set; }
    public FactionDescription Description { get; private set; }

    public static Faction Create(
        FactionName name,
        FactionDescription description)
    {
        return new Faction(
            id: FactionId.New(),
            name: name,
            description: description);
    }

    public void Update(
        FactionName name,
        FactionDescription description)
    {
        Name = name;
        Description = description;
    }
}
