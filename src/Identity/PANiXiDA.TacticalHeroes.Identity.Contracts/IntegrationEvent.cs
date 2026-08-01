namespace PANiXiDA.TacticalHeroes.Identity.Contracts;

public abstract record IntegrationEvent
{
    protected IntegrationEvent()
    {
        Id = Guid.CreateVersion7();
    }

    public Guid Id { get; init; }
    public required DateTimeOffset OccurredOnUtc { get; init; }
}
