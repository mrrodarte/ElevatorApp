using ElevatorDomain.Events;
using ElevatorDomain.Interfaces;
using ElevatorDomain.ValueObjects;

namespace ElevatorDomain.Entities
{
    //Entity properties and behaviors
    public class Elevator
    {
        #region Declarations
        public Guid Id { get; private set; } = Guid.NewGuid();
        public int CurrentFloor { get; private set; } = 1;
        public int MaxFloors { get; private set; }
        public Direction Direction { get; private set; } = Direction.None;
        public double Weight { get; private set; } = 0;
        public double MaxWeight { get; private set; }
        public ElevatorState State { get; private set; } = ElevatorState.Stopped;
        public List<int> RequestedFloors { get; private set; } = new List<int>();
        private readonly List<IDomainEvent> _domainEvents = new List<IDomainEvent>();
        //Added an IReadOnlyList for immutability of the list, the contents of the list
        //would not be readonly if we would just rely on the declaration above.
        public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;
        private readonly object _lockObject = new(); //ensures that multiple threads don't interfere with changing shared resources

        #endregion

        #region Constructors
        public Elevator(int maxFloors, double maxWeight)
        {
            MaxFloors = maxFloors;
            MaxWeight = maxWeight;
        }
        #endregion

        #region Methods and behaviors
        //Moves the elevator in the specific direction
        public async Task MoveOneFloor(Direction direction)
        {

            if (direction == Direction.Up)
            {
                CurrentFloor++;
            }
            else if (direction == Direction.Down)
            {
                CurrentFloor--;
            }

            //change elevator state
            State = ElevatorState.Moving;
            Direction = direction;

            //3 second delay is the elevator's time it takes to make the move
            await Task.Delay(TimeSpan.FromSeconds(3));

            //Add event to list to be dispatched
            _domainEvents.Add(new ElevatorMoved(DateTimeOffset.UtcNow, CurrentFloor));
        }

        //Stops the elevator
        public async Task Stop()
        {
            //change elevator state
            State = ElevatorState.Stopped;
            Direction = Direction.None;

            //it takes 1 second for the elevator after the stop
            await Task.Delay(TimeSpan.FromSeconds(1));

            //Add event to list to be dispatched
            _domainEvents.Add(new ElevatorStopped(DateTimeOffset.UtcNow, CurrentFloor));
        }

        // Passenger enters the elevator.
        public void AddPassenger(double passengerWeight)
        {
            Weight += passengerWeight;
        }

        // Passenger leaves the elevator.
        public void RemovePassenger(double passengerWeight)
        {
            Weight -= passengerWeight;
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
        #endregion
    }
}