using System.Reflection;
using System.Timers;
using ElevatorAPI.Factories;
using ElevatorDomain.Entities;
using ElevatorDomain.Interfaces;
using ElevatorDomain.Models;
using ElevatorDomain.Services;
using ElevatorDomain.Tasks;
using ElevatorDomain.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit.Abstractions;
namespace ElevatorSystem.Tests;

public class ElevatorTests 
{
    private readonly ITestOutputHelper? _output;
    private static Elevator? _sharedElevator;
    private readonly Elevator _elevator;
    private static ElevatorSettings _elevatorSettings = new ElevatorSettings { MaxFloors = 10, MaxWeight = 1000 };
    private IElevatorFactory _elevatorFactory;


    public ElevatorTests(ITestOutputHelper output)
    {
        _output = output;
        var options = Options.Create(_elevatorSettings);
        _elevatorFactory = new ElevatorFactory(options);
        _sharedElevator ??= _elevatorFactory.CreateElevator();
        _elevator = _sharedElevator;
    }

    [Fact]
    public async void MoveUp_ShouldIncreaseCurrentFloorByOne()
    {
        // Arrange
        // Create an elevator with 10 floors and 300 lbs pound limit
        //var elevator = new Elevator(10, 1000);
        var initialFloor = _elevator.CurrentFloor;

        //Act
        await _elevator.MoveOneFloor(Direction.Up);

        //Assert
        Assert.Equal(initialFloor + 1, _elevator.CurrentFloor);
    }

    [Fact]
    public async void MoveDown_ShouldDecreaseCurrentFloorByOne()
    {
        // Arrange
        // Create an elevator with 10 floors and 300 lbs pound limit
        //var elevator = new Elevator(10, 1000);
        var initialFloor = _elevator.CurrentFloor;

        //Act
        await _elevator.MoveOneFloor(Direction.Down);

        //Assert
        Assert.Equal(initialFloor - 1, _elevator.CurrentFloor);
    }

    //Had issues running my unit tests in isolation with theory / inline data
    //not sure if it has to do with most of my services are singleton
    //running my tests in separate methods have no issues. Will need to dig more into this issue
    //for now I kept them separated
    [Theory]
    [InlineData(3,150,Direction.Down)]
    [InlineData(5,150,Direction.Up)]
    public async void RequestElevator_ShouldMoveToRequestedFloor(int requestedFloor,double passengerWeight, Direction direction) //int requestedFloor,double passengerWeight, Direction direction
    {
        //Arrange
        //var requestedFloor = 3;
        //var passengerWeight = 150;
        //var direction = Direction.Down;

        // var elevatorSettings = new ElevatorSettings
        // {
        //     MaxFloors = 10,
        //     MaxWeight = 1000
        // };

        //var elevator = new Elevator(elevatorSettings.MaxFloors, elevatorSettings.MaxWeight);


        var mockElevatorFactory = new Mock<IElevatorFactory>();
        var mockLoggingService = new Mock<ILoggingService>();
        var mockDomainEventDispatcher = new Mock<IDomainEventDispatcher>();
        var mockLogger = new Mock<ILogger<Elevator>>();
        var mockElevatorTaskQueue = new Mock<IElevatorTaskQueue>();

        mockElevatorFactory.Setup(f => f.CreateElevator()).Returns(_elevator);
        mockLoggingService.Setup(c => c.LogEventAsync("logmessage")).Returns(Task.FromResult("logmessage"));
        mockDomainEventDispatcher = new Mock<IDomainEventDispatcher>();
        mockDomainEventDispatcher.Setup(d => d.DispatchEventsAsync(It.IsAny<List<IDomainEvent>>()))
                                 .Returns(Task.CompletedTask);

        //set up the ElevatorTaskQueue
        // Setup the Enqueue method to track tasks added to the queue
        List<KeyValuePair<int, MoveElevatorTask>> tasks = new List<KeyValuePair<int, MoveElevatorTask>>();
        mockElevatorTaskQueue.Setup(q => q.Enqueue(It.IsAny<int>(), It.IsAny<MoveElevatorTask>()))
                             .Callback<int, MoveElevatorTask>((floor, task) => tasks.Add(new KeyValuePair<int, MoveElevatorTask>(floor, task)));

        mockElevatorTaskQueue.Setup(q => q.HasTasks()).Returns(() => tasks.Any());


        // Setup the Dequeue method to remove tasks from the queue and return a boolean indicating success
        mockElevatorTaskQueue.Setup(q => q.Dequeue(It.IsAny<int>()))
                     .Returns((int floor) =>
                     {
                         var taskToRemove = tasks.FirstOrDefault(t => t.Key == floor);
                         if (!taskToRemove.Equals(default(KeyValuePair<int, MoveElevatorTask>)))
                         {
                             tasks.Remove(taskToRemove);
                             return true;
                         }
                         return false;
                     });

        mockElevatorTaskQueue.Setup(q => q.GetTask(It.IsAny<int>()))
                     .Returns((int floor) =>
                     {
                         return tasks.FirstOrDefault(kvp => kvp.Key == floor);
                     });

        // Setup the mock for MinFloorInQueue
        mockElevatorTaskQueue.Setup(q => q.MinFloorInQueue(It.IsAny<int>()))
                             .Returns((int floor) =>
                             {
                                 var floorsGreaterThanOrEqualToInput = tasks.Where(t => t.Key >= floor).Select(t => t.Key);
                                 return floorsGreaterThanOrEqualToInput.Any() ? floorsGreaterThanOrEqualToInput.Min() : -1;
                             });

        // Setup the mock for MaxFloorInQueue
        mockElevatorTaskQueue.Setup(q => q.MaxFloorInQueue(It.IsAny<int>()))
                             .Returns((int floor) =>
                             {
                                 var floorsLessThanOrEqualToInput = tasks.Where(t => t.Key <= floor).Select(t => t.Key);
                                 return floorsLessThanOrEqualToInput.Any() ? floorsLessThanOrEqualToInput.Max() : -1;
                             });



        var elevatorService = new ElevatorService(mockElevatorFactory.Object, mockLoggingService.Object,
            mockDomainEventDispatcher.Object, mockLogger.Object);

        var queueingService = new QueueingService(mockElevatorTaskQueue.Object, elevatorService,
            mockDomainEventDispatcher.Object, mockLoggingService.Object);

        // with the use of Reflection we are disabling the timer that processes tasks testing concurrent task processing
        // disabled for simplicity for the scope of this project
        var timerField = typeof(QueueingService).GetField("_processTasksTimer", BindingFlags.NonPublic | BindingFlags.Instance);
        if (timerField != null)
        {
            System.Timers.Timer? timer = timerField.GetValue(queueingService) as System.Timers.Timer;
            if (timer != null)
            {
                timer.Elapsed -= (ElapsedEventHandler)Delegate.CreateDelegate(
                    typeof(ElapsedEventHandler),
                    queueingService,
                    "ProcessTasksTimerElapsed"
                );
            }
        }

        var initialFloor = elevatorService.GetCurrentFloor();
        _output?.WriteLine($"Requested Floor {requestedFloor} - Elevator Current Floor {_elevator.CurrentFloor}");
        _output?.WriteLine($"Elevator Max F {_elevator.MaxFloors} - Elevator Max W {_elevator.MaxWeight}");

        //Act
        await queueingService.EnqueueTask(direction, requestedFloor, RequestType.OutsideRequest, passengerWeight);

        await queueingService.ProcessTasks();

        //Verify that the dequeue and enqueue have run at least once 
        mockElevatorTaskQueue.Verify(q => q.Dequeue(requestedFloor), Times.AtLeast(1));
        mockElevatorTaskQueue.Verify(q => q.GetTask(requestedFloor), Times.AtLeast(1));

        _output?.WriteLine($"Requested Floor {requestedFloor} - Elevator Current Floor {_elevator.CurrentFloor}");
        _output?.WriteLine($"Elevator Max F {_elevator.MaxFloors} - Elevator Max W {_elevator.MaxWeight}");

        //Assert
        Assert.Equal(requestedFloor, _elevator.CurrentFloor);


    }

    // [Theory]
    // [InlineData(5,150,Direction.Up)]
    // public async void RequestElevator_ShouldMoveToRequestedFloor2()  //int requestedFloor,double passengerWeight, Direction direction
    // {
    //     //Arrange
    //     var requestedFloor = 5;
    //     var passengerWeight = 150;
    //     var direction = Direction.Up;

    //     // var elevatorSettings = new ElevatorSettings
    //     // {
    //     //     MaxFloors = 10,
    //     //     MaxWeight = 1000
    //     // };

    //     //var elevator = new Elevator(elevatorSettings.MaxFloors, elevatorSettings.MaxWeight);


    //     var mockElevatorFactory = new Mock<IElevatorFactory>();
    //     var mockLoggingService = new Mock<ILoggingService>();
    //     var mockDomainEventDispatcher = new Mock<IDomainEventDispatcher>();
    //     var mockLogger = new Mock<ILogger<Elevator>>();
    //     var mockElevatorTaskQueue = new Mock<IElevatorTaskQueue>();

    //     mockElevatorFactory.Setup(f => f.CreateElevator()).Returns(_elevator);
    //     mockLoggingService.Setup(c => c.LogEventAsync("logmessage")).Returns(Task.FromResult("logmessage"));
    //     mockDomainEventDispatcher = new Mock<IDomainEventDispatcher>();
    //     mockDomainEventDispatcher.Setup(d => d.DispatchEventsAsync(It.IsAny<List<IDomainEvent>>()))
    //                              .Returns(Task.CompletedTask);

    //     //set up the ElevatorTaskQueue
    //     // Setup the Enqueue method to track tasks added to the queue
    //     List<KeyValuePair<int, MoveElevatorTask>> tasks = new List<KeyValuePair<int, MoveElevatorTask>>();
    //     mockElevatorTaskQueue.Setup(q => q.Enqueue(It.IsAny<int>(), It.IsAny<MoveElevatorTask>()))
    //                          .Callback<int, MoveElevatorTask>((floor, task) => tasks.Add(new KeyValuePair<int, MoveElevatorTask>(floor, task)));

    //     mockElevatorTaskQueue.Setup(q => q.HasTasks()).Returns(() => tasks.Any());


    //     // Setup the Dequeue method to remove tasks from the queue and return a boolean indicating success
    //     mockElevatorTaskQueue.Setup(q => q.Dequeue(It.IsAny<int>()))
    //                  .Returns((int floor) =>
    //                  {
    //                      var taskToRemove = tasks.FirstOrDefault(t => t.Key == floor);
    //                      if (!taskToRemove.Equals(default(KeyValuePair<int, MoveElevatorTask>)))
    //                      {
    //                          tasks.Remove(taskToRemove);
    //                          return true;
    //                      }
    //                      return false;
    //                  });

    //     mockElevatorTaskQueue.Setup(q => q.GetTask(It.IsAny<int>()))
    //                  .Returns((int floor) =>
    //                  {
    //                      return tasks.FirstOrDefault(kvp => kvp.Key == floor);
    //                  });

    //     // Setup the mock for MinFloorInQueue
    //     mockElevatorTaskQueue.Setup(q => q.MinFloorInQueue(It.IsAny<int>()))
    //                          .Returns((int floor) =>
    //                          {
    //                              var floorsGreaterThanOrEqualToInput = tasks.Where(t => t.Key >= floor).Select(t => t.Key);
    //                              return floorsGreaterThanOrEqualToInput.Any() ? floorsGreaterThanOrEqualToInput.Min() : -1;
    //                          });

    //     // Setup the mock for MaxFloorInQueue
    //     mockElevatorTaskQueue.Setup(q => q.MaxFloorInQueue(It.IsAny<int>()))
    //                          .Returns((int floor) =>
    //                          {
    //                              var floorsLessThanOrEqualToInput = tasks.Where(t => t.Key <= floor).Select(t => t.Key);
    //                              return floorsLessThanOrEqualToInput.Any() ? floorsLessThanOrEqualToInput.Max() : -1;
    //                          });



    //     var elevatorService = new ElevatorService(mockElevatorFactory.Object, mockLoggingService.Object,
    //         mockDomainEventDispatcher.Object, mockLogger.Object);

    //     var queueingService = new QueueingService(mockElevatorTaskQueue.Object, elevatorService,
    //         mockDomainEventDispatcher.Object, mockLoggingService.Object);

    //     // with the use of Reflection we are disabling the timer that processes tasks testing concurrent task processing
    //     // disabled for simplicity for the scope of this project
    //     var timerField = typeof(QueueingService).GetField("_processTasksTimer", BindingFlags.NonPublic | BindingFlags.Instance);
    //     if (timerField != null)
    //     {
    //         System.Timers.Timer? timer = timerField.GetValue(queueingService) as System.Timers.Timer;
    //         if (timer != null)
    //         {
    //             timer.Elapsed -= (ElapsedEventHandler)Delegate.CreateDelegate(
    //                 typeof(ElapsedEventHandler),
    //                 queueingService,
    //                 "ProcessTasksTimerElapsed"
    //             );
    //         }
    //     }

    //     var initialFloor = elevatorService.GetCurrentFloor();
    //     _output?.WriteLine($"Requested Floor {requestedFloor} - Elevator Current Floor {_elevator.CurrentFloor}");
    //     _output?.WriteLine($"Elevator Max F {_elevator.MaxFloors} - Elevator Max W {_elevator.MaxWeight}");

    //     //Act
    //     await queueingService.EnqueueTask(direction, requestedFloor, RequestType.OutsideRequest, passengerWeight);

    //     await queueingService.ProcessTasks();

    //     //Verify that the dequeue and enqueue have run at least once 
    //     mockElevatorTaskQueue.Verify(q => q.Dequeue(requestedFloor), Times.AtLeast(1));
    //     mockElevatorTaskQueue.Verify(q => q.GetTask(requestedFloor), Times.AtLeast(1));

    //     _output?.WriteLine($"Requested Floor {requestedFloor} - Elevator Current Floor {_elevator.CurrentFloor}");
    //     _output?.WriteLine($"Elevator Max F {_elevator.MaxFloors} - Elevator Max W {_elevator.MaxWeight}");

    //     //Assert
    //     Assert.Equal(requestedFloor, _elevator.CurrentFloor);

    // }
}