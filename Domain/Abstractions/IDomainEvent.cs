namespace Domain.Abstractions;

public interface IDomainEvent
{
    DateTime OccurredOnUtc { get; }
}

public abstract record DomainEventBase(DateTime OccurredOnUtc) : IDomainEvent;
