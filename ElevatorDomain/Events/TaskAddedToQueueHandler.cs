using ElevatorDomain.Interfaces;

namespace ElevatorDomain.Events
{
    public class TaskAddedToQueueHandler : IHandler<TaskAddedToQueue>
    {
        private readonly IQueueingService _queueingService;
        private readonly ILoggingService _loggingService;

        public TaskAddedToQueueHandler(IQueueingService queueingService,ILoggingService loggingService)
        {
            _queueingService = queueingService;
            _loggingService = loggingService;
        }

        public async Task Handle(TaskAddedToQueue domainEvent)
        {
            await _loggingService.LogEventAsync
                ($"{domainEvent.RequestType} from floor: {domainEvent.RequestedFromFloor} Direction ButtonPressed: {domainEvent.MoveDirection}");
            //await _queueingService.ProcessTasks();
        }
    }
}