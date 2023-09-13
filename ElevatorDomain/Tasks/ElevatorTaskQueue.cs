using System.Collections.Concurrent;
using ElevatorDomain.Interfaces;
using ElevatorDomain.ValueObjects;

namespace ElevatorDomain.Tasks
{
    public class ElevatorTaskQueue : IElevatorTaskQueue
    {

        //private readonly List<MoveElevatorTask> _up_tasks = new List<MoveElevatorTask>();
        //private readonly List<MoveElevatorTask> _down_tasks = new List<MoveElevatorTask>();
        //private PriorityQueue<MoveElevatorTask, int> _up_tasks = new PriorityQueue<MoveElevatorTask, int>();
        //private PriorityQueue<MoveElevatorTask, int> _down_tasks = new PriorityQueue<MoveElevatorTask, int>();

        private readonly ConcurrentDictionary<int,MoveElevatorTask> _tasks = new ConcurrentDictionary<int, MoveElevatorTask>();
        private readonly ILoggingService _logger;

        //To deal with concurrency and thread safety
        private readonly object _lockObject = new();

        public ElevatorTaskQueue(ILoggingService Logger)
        {
            _logger = Logger;
        }

        //Process to enqueue _tasks
        public void Enqueue(int requestedFloor, MoveElevatorTask task)
        {
            _tasks.TryAdd(requestedFloor,task);
        }

        //Return true if the item is removed
        public bool Dequeue(int floor)
        {
            return _tasks.TryRemove(floor, out _); 
        }

        public KeyValuePair<int,MoveElevatorTask> GetTask(int floor)
        {
            return _tasks.FirstOrDefault(kvp => kvp.Key == floor);
        }

        public KeyValuePair<int,MoveElevatorTask> GetInsideTask(RequestType requestType)
        {
            return _tasks.FirstOrDefault(kvp => kvp.Value.RequestType == requestType);
        }

        public Boolean FloorInQueue(int floor)
        {
            return _tasks.ContainsKey(floor);
        }

        public Boolean HasTasks()
        {
            return _tasks.Any();
        }

        public int MinFloorInQueue(int floor)
        {
            return _tasks.Keys.Where(key => key >= floor)
                .DefaultIfEmpty(-1)
                .Min();
        }

        public int MaxFloorInQueue(int floor)
        {
             return _tasks.Keys.Where(key => key <= floor)
                .DefaultIfEmpty(-1)
                .Max();
        }
    }

}