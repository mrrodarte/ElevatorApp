using ElevatorDomain.Entities;
using ElevatorDomain.ValueObjects;
using ElevatorDomain.Interfaces;
using ElevatorDomain.Models;
using ElevatorDomain.Tasks;
using Microsoft.Extensions.Logging;

namespace ElevatorDomain.Services
{
    public class ElevatorService : IElevatorService
    {
        private static Elevator? _sharedElevator;
        private readonly ILogger<Elevator> _logger;
        private readonly Elevator _elevator;
        private readonly ILoggingService _loggingService;
        private readonly IDomainEventDispatcher _domainEventDispatcher;

        public ElevatorService(IElevatorFactory elevatorFactory,
            ILoggingService loggingService,
            IDomainEventDispatcher domainEventDispatcher,
            ILogger<Elevator> logger)
        {
            // Check if the shared elevator instance exists. If not, create one.
            _sharedElevator ??= elevatorFactory.CreateElevator();
            _elevator = _sharedElevator;
            _loggingService = loggingService;
            _domainEventDispatcher = domainEventDispatcher;
            _logger = logger;
        }

        public async Task MoveOneFloor(Direction direction)
        {
            await _elevator.MoveOneFloor(direction); // elevator moves 1 floor at a time on the direction where it was requested

            // Dispatch the domain events
            await _domainEventDispatcher.DispatchEventsAsync(_elevator.DomainEvents);
            _elevator.ClearDomainEvents();
            
        }

        public async Task StopElevator()
        {
            await _elevator.Stop();
            // Dispatch the domain events
            await _domainEventDispatcher.DispatchEventsAsync(_elevator.DomainEvents);
            _elevator.ClearDomainEvents();
        }

        public async Task AddPassenger()
        {
            _elevator.AddPassenger(Passenger.Weight);
            await _loggingService.LogEventAsync($"Passenger entered. Current weight: {_elevator.Weight}.");
        }

        public async Task RemovePassenger()
        {
            _elevator.RemovePassenger(Passenger.Weight);
            await _loggingService.LogEventAsync($"Passenger exited. Current weight: {_elevator.Weight}.");
        }

        //these might seen redundant but its a way to get all the elevator settings from whoever requires it.
        public ElevatorSettings GetElevatorSettings()
        {
            return new ElevatorSettings
            {
                CurrentFloor = _elevator.CurrentFloor,
                MaxFloors = _elevator.MaxFloors,
                Weight = _elevator.Weight,
                MaxWeight = _elevator.MaxWeight,
                State = _elevator.State,
                Direction = _elevator.Direction,
            };
        }

        public int GetCurrentFloor() => _elevator.CurrentFloor;

        public double GetElevatorWeigt() => _elevator.Weight;

        public double GetElevatorMaxWeight() => _elevator.MaxWeight;

        public Direction GetCurrentDirection() => _elevator.Direction;

        public ElevatorState GetElevatorState() => _elevator.State;

        public int GetMaxFloors() => _elevator.MaxFloors;

        // ... any other methods related to the elevator's operations.
    }
}