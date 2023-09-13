using ElevatorDomain.Interfaces;
using ElevatorDomain.Services;
using ElevatorAPI.Services;
using ElevatorAPI.Events;
using ElevatorDomain.Events;
using ElevatorAPI.Factories;
using ElevatorDomain.Models;
using Microsoft.AspNetCore.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ElevatorDomain.Tasks;

namespace ElevatorAPI.Extensions
{
    public static class ApplicationServerExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services,
            IConfiguration config)
        {
            //services.AddCors();

            //Json serialization options
            services.Configure<JsonOptions>(opt =>
            {
                opt.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                opt.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            //This was going to be a background service to process elevator tasks, this added complexity that we might not
            //need for our demo.  Left the code for reference or in case we implement it in the future.
            //services.AddHostedService<ElevatorProcessingService>();

            //singleton service lifetime on the loggin service because we only need one instance
            //to log for the whole application
            services.AddSingleton<ILoggingService>(sp =>
                new LoggingService(config["LogFilePath"]!)
            );

            //Register domain events and services
            services.AddSingleton<IDomainEventDispatcher, DomainEventDispatcher>();
            services.AddSingleton<IHandler<ElevatorMoved>, ElevatorMovedHandler>();
            services.AddSingleton<IHandler<ElevatorStopped>, ElevatorStoppedHandler>();
            services.AddSingleton<IHandler<TaskAddedToQueue>, TaskAddedToQueueHandler>();
            services.AddSingleton<IElevatorTaskQueue, ElevatorTaskQueue>();

            //Register the elevator factory and service
            services.Configure<ElevatorSettings>(config.GetSection("ElevatorSettings"));
            services.AddSingleton<IElevatorFactory, ElevatorFactory>();
            services.AddSingleton<IElevatorService, ElevatorService>();
            services.AddSingleton<IQueueingService, QueueingService>();



            return services;
        }
    }
}