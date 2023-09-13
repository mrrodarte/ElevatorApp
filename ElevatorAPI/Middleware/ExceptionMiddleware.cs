using System.Net;
using System.Text.Json;
using ElevatorAPI.Errors;
using ElevatorDomain.Interfaces;

namespace ElevatorAPI.Middleware
{
    public class ExceptionMiddleware
    {
        readonly RequestDelegate _next;
        readonly ILogger<ExceptionMiddleware> _logger;
        readonly ILoggingService _loggingService;
        readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next,
                ILogger<ExceptionMiddleware> logger,
                ILoggingService loggingService,
                IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
            _loggingService = loggingService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                //Some very specific scenarios.
                //We need better error classification handling here, as almost all errors are returned as 500
                //explicitly left it as is for simplicity
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                if (ex.Message.Contains("Invalid InsideRequest"))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                }

                _logger.LogError(ex, ex.Message);
                await _loggingService.LogEventAsync(ex.Message);
                context.Response.ContentType = "application/json;application/xml";
                

                var response = _env.IsDevelopment()
                                ? new ApiException(context.Response.StatusCode,
                                ex.Message, ex.StackTrace?.ToString())
                                : new ApiException(context.Response.StatusCode,
                                ex.Message, string.Empty);

                //serialize the response to json string
                var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }
                );

                await context.Response.WriteAsync(json);
            }
        }
    }
}