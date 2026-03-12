namespace Fic.BuildingBlocks;

public abstract record DomainEvent(string EventType, DateTimeOffset OccurredAtUtc)
{
    public Guid EventId { get; init; } = Guid.NewGuid();
}
