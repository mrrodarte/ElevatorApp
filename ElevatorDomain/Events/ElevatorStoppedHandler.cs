using ElevatorDomain.Interfaces;

namespace ElevatorDomain.Events
{
    public class ElevatorStoppedHandler : IHandler<ElevatorStopped>
{
    private readonly ILoggingService _loggingService;

    public ElevatorStoppedHandler(ILoggingService loggingService)
    {
        _loggingService = loggingService;
    }

    public async Task Handle(ElevatorStopped domainEvent)
    {
        await _loggingService.LogEventAsync
            ($"Elevator stopped on floor {domainEvent.CurrentFloor}.");
    }
}}