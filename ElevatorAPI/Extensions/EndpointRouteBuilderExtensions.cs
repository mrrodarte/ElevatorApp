using System.Text.Encodings.Web;
using ElevatorAPI.EndpointHandlers;
using ElevatorDomain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ElevatorAPI.Extensions
{
    public static class EndpointRouteBuilderExtensions
    {
        //Use of extensions to avoid registering all endpoints in the program class
        public static void RegisterEndpoints(this IEndpointRouteBuilder endpointRouteBuilder,
        IConfiguration config)
        {
            //Group common url endpoints
            var elevatorEndpoints = endpointRouteBuilder.MapGroup(config["ApiUrl"]!);

            //endpoints
            elevatorEndpoints.MapPost("/floor", ElevatorHandlers.RequestFloor); //request a floor from outside
            elevatorEndpoints.MapGet("/settings", ElevatorHandlers.GetElevatorSettings); //return elevator settings and state
            elevatorEndpoints.MapGet("/processtatus",ElevatorHandlers.GetQueueProcessStatus); //return the state of the processing of queue tasks
            elevatorEndpoints.MapGet("/notifications/stream",ElevatorHandlers.NotificationStream); // one directional open channel to notify to the outside that our ProcessTasks is complete.
            
            //you would use and endpoint filters here in case you need

        }
    }
}