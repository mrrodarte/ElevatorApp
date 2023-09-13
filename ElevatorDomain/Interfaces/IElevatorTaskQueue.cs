using ElevatorDomain.Tasks;
using ElevatorDomain.ValueObjects;

namespace ElevatorDomain.Interfaces
{
    public interface IElevatorTaskQueue
    {
        public void Enqueue(int requestedFloor, MoveElevatorTask task);

        public bool Dequeue(int floor);

        public KeyValuePair<int,MoveElevatorTask> GetTask(int floor);

        public KeyValuePair<int,MoveElevatorTask> GetInsideTask(RequestType requestType);

        public Boolean FloorInQueue(int floor);

        public Boolean HasTasks();

        public int MinFloorInQueue(int floor);

        public int MaxFloorInQueue(int floor);
    }
}