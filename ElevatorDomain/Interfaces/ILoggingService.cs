namespace ElevatorDomain.Interfaces
{
    public interface ILoggingService
    {
        public Task LogEventAsync(string message);
    }
}