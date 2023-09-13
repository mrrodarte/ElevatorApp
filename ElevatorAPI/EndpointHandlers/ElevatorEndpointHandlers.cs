using Microsoft.AspNetCore.Http.HttpResults;
using ElevatorAPI.DTOs;
using ElevatorDomain.Interfaces;
using ElevatorDomain.Models;
using ElevatorDomain.ValueObjects;
using System.Text.Json;

namespace ElevatorAPI.EndpointHandlers
{
    //Complexity has been intentionally added to demonstrate the best practices of using DTOs
    //for making post responses, even though we could have used simple query strings
    //since our DTOs contain only 1-2 fields
    public static class ElevatorHandlers
    {
        public static async Task<Results<NoContent, BadRequest<string>>> RequestFloor
            (FloorRequestDto floorRequestDto, IQueueingService queueService,
                IElevatorService elevatorService)
        {
            //Validation, this could be implemented using custom validation attribute and use
            //data annotation to validate the dto model, but added manual validation here for simplicity
            if (IsInvalidFloor(floorRequestDto.RequestFromFloor, elevatorService.GetMaxFloors()))
                return TypedResults.BadRequest("Please enter a valid floor within range.");

            await queueService.EnqueueTask(floorRequestDto.Direction, floorRequestDto.RequestFromFloor,
                floorRequestDto.RequestType, floorRequestDto.PassengerWeight);

            // I left this commented code to show my initial thought process.  At first was adding 
            // a delay here, but figured this is behavior of the elevator entity.  api should not
            // concern about this nor handle it
            //simulate a 3 second delay
            //await Task.Delay(TimeSpan.FromSeconds(3));

            return TypedResults.NoContent();
        }

        //Endpoint to get elevator settings and state
        public static Ok<ElevatorSettings> GetElevatorSettings(IElevatorService elevatorService)
        {
            var settings = elevatorService.GetElevatorSettings();
            return TypedResults.Ok(settings);
        }

        public static Ok<ProcessStatus> GetQueueProcessStatus(IQueueingService queueingService)
        {
            var result = queueingService.GetProcessQueueStatus();
            return TypedResults.Ok(result);
        }

        //This was produced to send a notification stream to our client that the processing of tasks is done
        //we could have used different methods, tried simple pub / sub event subscription but it did not work as expected
        //also signalR to communicate, this seemed the simplest method
        public static async Task NotificationStream(HttpContext httpContext, IQueueingService queueingService)
        {
            string strData = string.Empty;
            var response = httpContext.Response;
            response.Headers.Add("Content-Type", "text/event-stream");

            var status = queueingService.GetProcessQueueStatus();

            switch (status)
            {
                case ProcessStatus.Completed : 
                    strData = "Completed";
                    break;
                case ProcessStatus.InProgress :
                    strData = "InProgress";
                    break;
                default:
                    strData = "Idle";
                    break;
            }

            //var data = new { Status = status, Message = "Process completed" };
            //string jsonData = JsonSerializer.Serialize(data);

            await response.WriteAsync(strData);
        }

        private static bool IsInvalidFloor(int floor, int maxFloors)
        {
            return (floor < 1 || floor > maxFloors);
        }
    }
}