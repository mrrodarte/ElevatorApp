using ElevatorDomain.Interfaces;

namespace ElevatorDomain.Events
{
    public class ElevatorStopped : IDomainEvent
    {
        public int CurrentFloor { get; }
        public DateTimeOffset TimeStamp { get; }

        public ElevatorStopped(DateTimeOffset timeStamp, int currentFloor)
        {
            TimeStamp = timeStamp;
            CurrentFloor = currentFloor;
        }
    }

}