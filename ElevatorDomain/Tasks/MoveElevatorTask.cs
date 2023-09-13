using ElevatorDomain.ValueObjects;

namespace ElevatorDomain.Tasks
{
    public class MoveElevatorTask
    {
        public int TargetFloor { get; private set; }
        public Direction RequestedDirection { get; private set; }
        public RequestType RequestType { get; private set; }
        public double PassengerWeight {get; private set;}
        public DateTimeOffset TimeStamp { get; private set; }

        public MoveElevatorTask(int destination, Direction direction, 
            RequestType requestType, double passengerWeight)
        {
            TargetFloor = destination;
            RequestedDirection = direction;
            RequestType = requestType;
            TimeStamp = DateTimeOffset.UtcNow;
            PassengerWeight = passengerWeight;
        }

        public void UpdateDirection(Direction newDirection)
        {
            RequestedDirection = newDirection;
        }
    }
}
