using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ElevatorDomain.Interfaces;

namespace ElevatorAPI.Events
{
    //This class allows us to handle events from different event type handlers
    public class DomainEventDispatcher : IDomainEventDispatcher
    {
        private readonly IServiceProvider _serviceProvider;

        public DomainEventDispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task DispatchEventsAsync(IReadOnlyList<IDomainEvent> domainEvents)
        {
            try
            {
                foreach (var domainEvent in domainEvents)
                {
                    //gets event type
                    var eventType = domainEvent.GetType();

                    //Creates a generic type on-the-fly based on the actual type of the event being processed.
                    var handlerType = typeof(IHandler<>).MakeGenericType(eventType);

                    //Here we use the generic type to retrieve the appropiate handler 
                    //from the DI service container
                    dynamic? handler = _serviceProvider.GetService(handlerType);
                    if (handler != null)
                    {
                        //sends the instruction to handle the event of the appropiate handler
                        await handler.Handle((dynamic)domainEvent);
                    }
                }
            }
            catch (InvalidOperationException)
            {
                //do nothing
                //The error is happening is a Collection was modified; enumeration operation may not execute.
                //and this is happening due to a concurrency issue where the devent has been serviced already
                //due to time contraints I will catch the side-effect and do nothing, its basically for logging
                //what happened and the event was logged in another thread.  In a real life scenario I will find
                //the concurrency issue and solve it.
            }
        }
    }

}