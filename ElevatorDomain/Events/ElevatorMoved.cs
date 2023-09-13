using ElevatorDomain.Interfaces;

namespace ElevatorDomain.Events
{
    public class ElevatorMoved : IDomainEvent
    {
        public int CurrentFloor { get; }
        public DateTimeOffset TimeStamp { get; }
        public ElevatorMoved(DateTimeOffset timeStamp, int currentFloor)
        {
            TimeStamp = timeStamp;
            CurrentFloor = currentFloor;
        }
    }

}