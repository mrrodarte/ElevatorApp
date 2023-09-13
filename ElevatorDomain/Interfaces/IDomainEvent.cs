namespace ElevatorDomain.Interfaces
{
    public interface IDomainEvent
    {
        //This is a marker interface for domain events.  This is a simple POCO (Plain Old CLR Object)
        //class that get instantiated and passed around.
        //Reason: It ensures that only valid domain events (those that implement this marker interface) 
        //can be passed around and dispatched. This creates a level of type safety 
        //so you don't accidentally dispatch something that isn't a domain event.
    }
}