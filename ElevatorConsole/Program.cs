using System.Text.RegularExpressions;
using ElevatorAPI.DTOs;
using ElevatorDomain.ValueObjects;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Builder;
using System.Diagnostics;
using ElevatorAPI.Services;
using ElevatorDomain.Interfaces;
using ElevatorDomain.Services;
using ElevatorDomain.Models;
using Microsoft.Extensions.Hosting;

namespace ElevatorConsole
{
    public class Program
    {
        //private IQueueingService? _queueService;

        private static string ApiUrl = string.Empty;
        private static string processStatus = string.Empty;
        private static ILoggingService? _loggingService;

        public static async Task Main(string[] args)
        {
            //Read api url from appSettings.json
            var builder = WebApplication.CreateBuilder(args);

            ApiUrl = builder.Configuration["ApiUrlHost"] + builder.Configuration["ApiUrl"];
            _loggingService = new LoggingService(builder.Configuration["LogFilePath"]!);

            try
            {
                //Start the api
                Process.Start("ElevatorAPI.exe", "--silent");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start the API. Please contact support : {ex.Message}");
            }


            Console.Clear();
            Console.WriteLine("Welcome to Super Mario's Elevator Simulator!");
            Console.WriteLine("================================================================================================================");
            Console.WriteLine("--IMPORTANT-- notes on how to run the elevator:");
            Console.WriteLine("Please enter the floor you are making the call from and your intended direction (e.g 5U, 2D). Enter Q to quit.");
            Console.WriteLine("Valid entries for Outside Requests [floor number][Desired Direction]");
            Console.WriteLine("Valid entries for Inside Requests  [floor number]");
            Console.WriteLine("Examples:");
            Console.WriteLine("5U : outside request from floor 5 want to go up.");
            Console.WriteLine("3 : inside request want to stop at floor 3. (Direction depends on where the elvator is going).");
            Console.WriteLine("When you make a call to the elevator and it reaches your floor you will automatically board.");
            Console.WriteLine("After that you can select your destination. Once it reaches your floor, you will unboard automatically.");
            Console.WriteLine("May you have a happy journey to the mushroom kingdom.");
            Console.WriteLine("================================================================================================================");
            Console.WriteLine("--IMPORTANT NOTES for Admin-- Be aware that there is a queueing service that handles all of");
            Console.WriteLine("  the elevator requests.  You might not get an immediate response if there are issues handling");
            Console.WriteLine("  your request.  Always look at the logs for details for possible troubleshooting issues.");
            Console.WriteLine("================================================================================================================");
            Console.WriteLine("");
            //For simplicity requiring less entries between requests we are assuming all passengers will be this set weight
            var Weight = GetValidDoubleInput("What is your aprox weight (sorry for asking this, elevator requirements)");

            //await CheckQueueProcessStatus();

            while (true) // or some condition to exit
            {
                Console.WriteLine("Make an elevator request: Building floors ([1-10][U/D for an outside request])");
                var request = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(request)) continue;

                //End the elevator system
                if (request.ToUpper() == "Q")
                {
                    while (true)
                    {
                        await CheckQueueProcessStatus();
                        await Task.Delay(1000); // check every second after the Quit command
                        Console.Write(".");
                        if (processStatus != "InProgress")
                            Environment.Exit(0);
                    }
                }

                //var match = Regex.Match(request, @"(?<number>10|[1-9])(?<direction>[UDud])(?<requesttype>[OIoi])");

                var match = Regex.Match(request, @"^(?:(?<number>10|[1-9])(?<direction>[UDud])|(?<number>10|[1-9]))$");


                //initial default values
                int requestedFloor = 1;
                Direction requestedDirection = Direction.None;
                RequestType requestType = RequestType.InsideRequest;
                if (match.Success)
                {
                    requestedFloor = int.Parse(match.Groups["number"].Value);

                    if (match.Groups["direction"].Success)
                    {
                        requestedDirection = match.Groups["direction"].Value?[0].ToString().ToUpper() == "U"
                                                ? Direction.Up
                                                : Direction.Down;
                    }

                    if (match.Groups["requesttype"].Success)
                    {
                        requestType = match.Groups["requesttype"].Value?[0].ToString().ToUpper() == "O"
                                                ? RequestType.OutsideRequest
                                                : RequestType.InsideRequest;
                    }

                    if (requestedDirection != Direction.None)
                        requestType = RequestType.OutsideRequest;
                }
                else
                {
                    Console.WriteLine("Please enter a valid input. You can refer to the usage notes above.");
                    continue;
                }

                // This needs to be handled by the application layer queueing service. Left to show thought process
                // //If elevator has no weight, it means no one is inside, you can only make OutsideRequests
                // if (requestType == RequestType.InsideRequest && elevatorSettings.Weight <= 0)
                // {
                //     Console.WriteLine("There is no one inside the elevator, only outside requests can be made.");
                //     continue;
                // }

                Console.WriteLine($"Your request: Floor {requestedFloor}, Desire Direction: {requestedDirection}," +
                    $"Request Type: {requestType}");

                Console.WriteLine("Is this correct (Y/N)?");
                var confirm = Console.ReadLine()!.ToUpper();
                if (confirm == "N") continue;

                //set dto
                FloorRequestDto floorRequestDto = new FloorRequestDto()
                {
                    RequestFromFloor = requestedFloor,
                    Direction = requestedDirection,
                    RequestType = requestType,
                    PassengerWeight = Weight
                };

                await MakeElevatorRequest(floorRequestDto);

            }
        }

        private static async Task MakeElevatorRequest(FloorRequestDto floorRequestDto)
        {
            string json = JsonSerializer.Serialize(floorRequestDto);

            using (HttpClient client = new HttpClient())
            {
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                try
                {

                    HttpResponseMessage response = await client.PostAsync(ApiUrl + "/floor", content);
                    string responseBody = await response.Content.ReadAsStringAsync();
                    if (!response.IsSuccessStatusCode)
                    {
                        //A more friendly response to the user, log the more detailed one for troubleshooting purposes.
                        //this could be revised for better handling by status codes
                        string errorMessage = responseBody switch
                        {
                            string s when s.Contains("reange") => "Please enter a valid floor within range.",
                            string s when s.Contains("Invalid InsideRequest") => "Your inside request might be invalid, "
                                + "there might be no one inside to make this request. please check the logs for details.",
                            string s when s.Contains("Weight Limit") => "Weight limit exceeded. Only inside requests allowed",
                            _ => "An unexpected error occurred.  Please check the logs for details."
                        };


                        Console.WriteLine(errorMessage);

                        await _loggingService!.LogEventAsync($"{response.StatusCode}: {responseBody}");
                    }
                    else
                    {
                        Console.WriteLine("Elevator request call succeded.");
                    }

                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("An unexpected error occurred.  Please check the logs for details.");
                    await _loggingService!.LogEventAsync($"{e.Message}");
                }
                catch (Exception e)
                {
                    Console.WriteLine("An unexpected error occurred.  Please check the logs for details.");
                    await _loggingService!.LogEventAsync($"{e.Message}");
                }
            }
        }

        private static async Task<ElevatorSettings> GetElevatorSettings()
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {

                    HttpResponseMessage response = await client.GetAsync(ApiUrl + "/settings");
                    string responseBody = await response.Content.ReadAsStringAsync();

                    ElevatorSettings settings = JsonSerializer.Deserialize<ElevatorSettings>(responseBody)!;

                    return settings;
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("An unexpected error occured. Please contact support.");
                    await _loggingService!.LogEventAsync($"{e.Message}");
                    return new ElevatorSettings(); //return a default value
                }
            }
        }

        private static async Task CheckQueueProcessStatus()
        {
            using (HttpClient client = new HttpClient())
            {
                string apiEndPoint = ApiUrl;
                try
                {
                    using var stream = await client.GetStreamAsync(ApiUrl + "/notifications/stream");

                    using var reader = new StreamReader(stream);

                    while (!reader.EndOfStream)
                    {
                        var line = await reader.ReadLineAsync();

                        if (!string.IsNullOrEmpty(line))
                        {
                            processStatus = line;
                            //Console.WriteLine($"Status from stream {line}");
                        }
                    }
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("An unexpected error occured. Please contact support.");
                    await _loggingService!.LogEventAsync($"{e.Message}");
                }
            }
        }

        //Guard pattern
        static int GetValidNumberInput(string prompt)
        {
            int? result = null;

            while (!result.HasValue)
            {
                Console.WriteLine(prompt);
                if (int.TryParse(Console.ReadLine(), out int number))
                {
                    result = number;
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter a valid number.");
                }
            }

            return result.Value;
        }

        static double GetValidDoubleInput(string prompt)
        {
            double? result = null;

            while (!result.HasValue)
            {
                Console.WriteLine(prompt);
                if (double.TryParse(Console.ReadLine(), out double number))
                {
                    result = number;
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter a valid number.");
                }
            }

            return result.Value;
        }

    }
}
