
using ElevatorDomain.Events;
using ElevatorDomain.Interfaces;
using ElevatorDomain.Tasks;
using ElevatorDomain.ValueObjects;

namespace ElevatorDomain.Services
{
    public partial class QueueingService : IQueueingService
    {
        //SemaphoreSlim ensures that only one thread is processing tasks at a time
        //private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly object _lockObject = new(); //ensures that multiple threads don't interfere with changing shared resources
        private readonly System.Timers.Timer _processTasksTimer;
        private ProcessStatus _currentStatus = ProcessStatus.Idle;
        public delegate void ProcessCompletedEventHandler(object sender, EventArgs e);
        public event ProcessCompletedEventHandler? ProcessCompleted;  // Define the event using the delegate, in case needs to subscribed will leave it for reference
        private readonly ILoggingService _logger;
        private readonly IElevatorTaskQueue _taskQueue;
        private readonly IElevatorService _elevatorService;
        private readonly IDomainEventDispatcher _domainEventDispatcher;
        private readonly List<IDomainEvent> _domainEvents = new List<IDomainEvent>();
        public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        public QueueingService(IElevatorTaskQueue taskQueue,
            IElevatorService elevatorService, IDomainEventDispatcher domainEventDispatcher,
            ILoggingService logger)
        {
            _taskQueue = taskQueue;
            _elevatorService = elevatorService;
            _domainEventDispatcher = domainEventDispatcher;
            _logger = logger;

            _processTasksTimer = new System.Timers.Timer(3000); //fixed time in seconds for the process to start. this could be added in config, hardcoded for simplicity. 
            //Subscribe to the Elapsed event.
            _processTasksTimer.Elapsed += ProcessTasksTimerElapsed!;
            _processTasksTimer.Start();
        }

        public async Task EnqueueTask(Direction requestedDirection, int requestedFloor,
            RequestType requestType, double passengerWeight)
        {
            //Possible violation of DIP, aware of it but allowed for simplicity
            //as in our example it is unlikely MoveElevatorTask (Plain Old CLR Object) 
            //is going to change.
            var task = new MoveElevatorTask(requestedFloor, requestedDirection, requestType, passengerWeight);

            //An inside request was made, but elevator weight is zero meaning there is no one inside
            if (requestType == RequestType.InsideRequest
                && _elevatorService.GetElevatorWeigt() <= 0)
            {
                await _logger.LogEventAsync($"Inside requests can't be made until a passenger boards the elevator. Request was not queued.");
                throw new Exception("Invalid InsideRequest");
            }

            _taskQueue.Enqueue(requestedFloor, task);

            //Raise the event, need to be done by request
            lock (_lockObject)
            {
                _domainEvents.Add(new TaskAddedToQueue(DateTimeOffset.UtcNow, requestType,
                                requestedDirection, requestedFloor));

            }

            await _domainEventDispatcher.DispatchEventsAsync(_domainEvents);
            _domainEvents.Clear();
        }

        public async Task ProcessTasks()
        {
            //Check if there are any tasks if there are not tasks or its already processing nothing to do here
            if (!_taskQueue.HasTasks() ||
                _currentStatus == ProcessStatus.InProgress)
            {
                //await _logger.LogEventAsync($"There are no tasks or process is in progress already.");
                return;
            }

            //start the process, init values
            _currentStatus = ProcessStatus.InProgress;
            KeyValuePair<int, MoveElevatorTask> task = new KeyValuePair<int, MoveElevatorTask>();
            Direction targetDirection = _elevatorService.GetCurrentDirection();
            int currTargetFloor = 1;
            bool loggedOnce = false;

            while (currTargetFloor != _elevatorService.GetCurrentFloor() || _taskQueue.HasTasks())
            {
                (Direction currentDirection, int currentFloor,
                                    ElevatorState elevatorState) = GetElevatorSettings();

                //*** IF ELEVATOR UP look for the min floor task that is greater the current floor
                //** IF ELEVATOR DOWN loop for the max floor task that is less than the current floor
                var taskFloor = _taskQueue.MinFloorInQueue(currentFloor);
                if (taskFloor < 0)
                    taskFloor = _taskQueue.MaxFloorInQueue(currentFloor);

                //No more tasks break operation
                if (taskFloor > 0)
                {
                    task = _taskQueue.GetTask(taskFloor);

                    if (_elevatorService.GetElevatorWeigt() >= _elevatorService.GetElevatorMaxWeight()
                         && task.Value.RequestType == RequestType.OutsideRequest)
                    {
                        task = _taskQueue.GetInsideTask(RequestType.InsideRequest);

                        if (task.Key <= 0 || task.Value == null)
                        {
                            if (!loggedOnce)
                            {
                                await _logger.LogEventAsync($"Elevator Max Weight at its limit ... waiting for an inside request.");
                                loggedOnce = true;
                            }
                            continue;
                        }
                    }

                    //set the target floor
                    currTargetFloor = task.Key; //10
                    targetDirection = task.Value.RequestedDirection; //up

                    //compare relative position of current elevator floor
                    if (currTargetFloor > currentFloor &&
                        currentDirection != Direction.Down)
                    {
                        targetDirection = Direction.Up;
                    }
                    else if (currTargetFloor < currentFloor &&
                            currentDirection != Direction.Up)
                    {
                        targetDirection = Direction.Down;
                    }

                    //same floor press simulate a destination stop so we can dequeue
                    if (currTargetFloor == _elevatorService.GetCurrentFloor())
                    {
                        await ElevatorReachedDestination(currTargetFloor, _taskQueue);
                        continue;
                    }
                    else
                    {
                        //Ask the elevator service or manager to move the elvator one floor in the direction
                        await _elevatorService.MoveOneFloor(targetDirection);
                        loggedOnce = false;
                    }
                }

                //Elevator has reach one of its destinations
                if (currTargetFloor == _elevatorService.GetCurrentFloor())
                {
                    await ElevatorReachedDestination(currTargetFloor, _taskQueue);
                }
            }
            _currentStatus = ProcessStatus.Completed;
            //raise the event so that our UI interface subscribed to this method get the notice that the process is complete.
            OnProcessCompleted();
        }

        //event in case needs to subscribed will leave it for reference
        protected virtual void OnProcessCompleted()
        {
            ProcessCompleted?.Invoke(this, EventArgs.Empty);
        }
        
        private async Task ElevatorReachedDestination(int currTargetFloor, IElevatorTaskQueue _taskQueue)
        {
            //Elevator has reach one of its destinations
            await _elevatorService.StopElevator();

            //await _logger.LogEventAsync($"Task Dequeued, targetFloor: {currTargetFloor}. HasTasks?: {_taskQueue.HasTasks()}");

            var task = _taskQueue.GetTask(currTargetFloor);

            //this the rare case of an operation where the same floor was pushed and passenger got out
            //just remove the passenger
            if (task.Key <= 0 || task.Value == null)
            {
                await _elevatorService.RemovePassenger();
            }
            else //normal operation
            {
                //If the request is an outside request board the passenger
                if (task.Value.RequestType == RequestType.OutsideRequest)
                {
                    Passenger.SetWeight(task.Value.PassengerWeight);
                    await _elevatorService.AddPassenger();
                }
                else if (task.Value.RequestType == RequestType.InsideRequest)
                {
                    await _elevatorService.RemovePassenger();
                }
            }

            _taskQueue.Dequeue(currTargetFloor);
        }

        //timer to process tasks
        private async void ProcessTasksTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            await ProcessTasks();
        }

        private (Direction direction, int currentFloor, ElevatorState elevatorState) GetElevatorSettings()
        {
            var elevatorSettings = _elevatorService.GetElevatorSettings();
            return (elevatorSettings.Direction, elevatorSettings.CurrentFloor,
                elevatorSettings.State);
        }

        public ProcessStatus GetProcessQueueStatus()
        {
            return _currentStatus;
        }
    }
}