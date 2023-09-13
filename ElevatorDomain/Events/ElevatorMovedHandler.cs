using ElevatorDomain.Interfaces;
using ElevatorDomain.Events;

namespace ElevatorDomain.Events
{
    public class ElevatorMovedHandler : IHandler<ElevatorMoved>
{
    private readonly ILoggingService _loggingService;

    public ElevatorMovedHandler(ILoggingService loggingService)
    {
        _loggingService = loggingService;
    }

    public async Task Handle(ElevatorMoved domainEvent)
    {
        await _loggingService.LogEventAsync
            ($"Elevator moved to floor {domainEvent.CurrentFloor}");
    }
}}