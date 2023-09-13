using ElevatorAPI.Extensions;
using ElevatorAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

//Run the app silently when it runs from our Elevator Console App in production if we want to
var isSilent = args.Contains("--silent");
if (isSilent)
{
    builder.Logging.ClearProviders(); 
    builder.Logging.SetMinimumLevel(LogLevel.Error); // Or LogLevel.None to completely silence logging
}

// Add services to the container.

//** We don't need controllers we are using a minimal api for our demo
//builder.Services.AddControllers();

//** No swagger needed for our demo, we will document the usage of our app on our demo console
//The api is decoupled from the console and could be use with any other presentation layer
//in a real case scenario we would provide documentation for the use of our elevator api.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//Register Services, using an extension method for cleaner code modules
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

// Set the URLs
app.Urls.Clear(); // Clear any default URLs
app.Urls.Add(builder.Configuration["ApiUrlHost"]!); // Add the desired URL to launch settings json file

//Application Middleware for Exception Handling in One Place
app.UseMiddleware<ExceptionMiddleware>();

// Configure the HTTP request pipeline. ** see above notes.
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

//We are using plain http for our demo purposes, in a real world scenario we would secure
//our api transport using https
//app.UseHttpsRedirection();

//Endpoints added to demonstrate using extension and handlers to map endpoints
//and remove endpoint mappings from program.cs  If you have a lot of endpoints with logic in them
//the program class can get messy, not in our demo case but I still mapped them this way
app.RegisterEndpoints(builder.Configuration);

//This is where I would use authentication and authorization in case the api needs to be secured
//and roles would be managed using least privilege principle
//app.UseAuthentication();
//app.UseAuthorization();

app.Run();
