using ElevatorDomain.Interfaces;
using ElevatorDomain.Tasks;
using ElevatorDomain.ValueObjects;

namespace ElevatorDomain.Events
{
    public class TaskAddedToQueue : IDomainEvent
    {
        public DateTimeOffset TimeStamp { get; }
        public RequestType RequestType { get; }
        public Direction MoveDirection { get; }
        public int RequestedFromFloor { get; }

        public TaskAddedToQueue(DateTimeOffset timeStamp, RequestType requestType, Direction moveDirection,int requestedFromFloor)
        {
            TimeStamp = timeStamp;
            RequestType = requestType;
            MoveDirection = moveDirection;
            RequestedFromFloor = requestedFromFloor;
        }
    }
}