using System;
using System.IO;
using ElevatorDomain.Interfaces;

namespace ElevatorAPI.Services
{
    public class LoggingService : ILoggingService
    {
        private readonly string _logFilePath;
        private readonly SemaphoreSlim _asyncLockObj = new SemaphoreSlim(1, 1);

        public LoggingService(string logFilePath)
        {
            _logFilePath = logFilePath ?? throw new ArgumentNullException(nameof(logFilePath));

            // Ensure the directory exists
            string directoryPath = Path.GetDirectoryName(_logFilePath)!;
            Directory.CreateDirectory(directoryPath);

            //for now clear file before each run
            File.WriteAllTextAsync(_logFilePath, string.Empty);
        }

        public async Task LogEventAsync(string message)
        {
            //we could use DateTimeOffset if we wanted UTC time, will leave for simplicity
            string logMessage = $"{DateTime.Now}: {message}";

            //ensure thread-safety
            await _asyncLockObj.WaitAsync();
            try
            {
                await File.AppendAllTextAsync(_logFilePath, logMessage + Environment.NewLine);
            }
            finally
            {
                _asyncLockObj.Release();
            }
        }
    }
}