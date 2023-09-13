using ElevatorDomain.Models;
using ElevatorDomain.Tasks;
using ElevatorDomain.ValueObjects;

namespace ElevatorDomain.Interfaces
{
    public interface IElevatorService
    {
        // public Task MoveToFloor(Direction direction, int floor, MoveType moveType, 
        //     DateTimeOffset timeStamp);

        public Task MoveOneFloor(Direction direction);

        public Task StopElevator();

        public Task AddPassenger();

        public Task RemovePassenger();

        public ElevatorState GetElevatorState();

        public ElevatorSettings GetElevatorSettings();

        public int GetMaxFloors();

        public int GetCurrentFloor();

        public Direction GetCurrentDirection();

        public double GetElevatorWeigt();

        public double GetElevatorMaxWeight();
    }
}