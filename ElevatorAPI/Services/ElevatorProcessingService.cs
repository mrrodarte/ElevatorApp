using ElevatorDomain.Interfaces;

namespace ElevatorAPI.Services
{
    //This was going to be a background service to process elevator tasks, this added complexity that we might not
    //need for our demo.  Left the code for reference or in case we implement it in the future.
    public class ElevatorProcessingService : BackgroundService
{
    private readonly IQueueingService _queueingService;

    public ElevatorProcessingService(IQueueingService queueingService)
    {
        _queueingService = queueingService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _queueingService.ProcessTasks(); 
            await Task.Delay(1000, stoppingToken); // This delay is to prevent CPU from spiking up. Adjust as needed.
        }
    }
}
}