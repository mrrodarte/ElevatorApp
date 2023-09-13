namespace ElevatorDomain.Interfaces
{
    // contravariance we allow for the interface to use a base type in place of a derived one
    // while working with event handler interfaces and delegates.
    // In our case we could have a handler that handles a base event type, so we allow this 
    // flexibility here.
    public interface IHandler<in TEvent>
    {
        public Task Handle(TEvent domainEvent);
    }

}