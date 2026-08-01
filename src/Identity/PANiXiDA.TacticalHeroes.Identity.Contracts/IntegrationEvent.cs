namespace PANiXiDA.TacticalHeroes.Identity.Contracts;

public abstract record IntegrationEvent
{
    protected IntegrationEvent()
    {
        Id = Guid.CreateVersion7();
        OccurredOnUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; init; }
    public DateTimeOffset OccurredOnUtc { get; init; }
}
