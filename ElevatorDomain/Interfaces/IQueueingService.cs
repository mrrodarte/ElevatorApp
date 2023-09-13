using ElevatorDomain.Services;
using ElevatorDomain.ValueObjects;

namespace ElevatorDomain.Interfaces
{
    public interface IQueueingService
    {
        public Task EnqueueTask(Direction direction, int floor, RequestType requestType, double passengerWeight);
        public Task ProcessTasks();

        public ProcessStatus GetProcessQueueStatus();

        event QueueingService.ProcessCompletedEventHandler ProcessCompleted; // event in case someone needs to subscribed

    }
}