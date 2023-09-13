namespace ElevatorDomain.Interfaces
{
    public interface IDomainEventDispatcher
    {
        //Left this commented code to show my thought process, initially I was going to handle
        //raising events as they happened. 
        //Use of generics and generics constraints
        //void Raise<TEvent>(TEvent domainEvent) where TEvent : class;

        //The new method is more useful since want to ensure that all primary operations succeed before 
        //any side-effects (like logging) take place
        public Task DispatchEventsAsync(IReadOnlyList<IDomainEvent> domainEvents);
    }
}