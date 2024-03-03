using API1;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ILogger = Serilog.ILogger;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var externalApiBaseUrl = Environment.GetEnvironmentVariable("EXTERNAL_API_URL") ?? "http://localhost:5002";
//builder.Configuration["API1URL"] = externalApiBaseUrl;

// Add logging 
builder.Logging.ClearProviders();
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Get the Pong API URL
//var pongAPI = builder.Configuration.GetValue<string>("API1URL");
builder.Services.AddHttpClient("APIClient", c => { c.BaseAddress = new Uri(externalApiBaseUrl); });


var app = builder.Build();


app.MapGet("/hello", ([FromQuery] string name) =>
{
    return $"Hello {name}";
});


// Send endpoint that posts to Pong and sends some data back 
app.MapPost("/", async (Message todo, IHttpClientFactory httpFactory, ILogger logger) => {
    if (todo is not null)
    {
        todo.Data= todo.Data + " - Tested";
    }

    var data = JsonSerializer.Serialize(todo);
    
    //httpClient.BaseAddress = new Uri("http://host.docker.internal:5050");


    var httpClient = httpFactory.CreateClient("APIClient");

    logger.Information("==========================================================");
    logger.Information(httpClient.BaseAddress.ToString());

    // Check if the directory exists, if not, create it
    var curDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
    //string directoryPath = Path.GetDirectoryName(curDir);
    if (!Directory.Exists(curDir))
    {
        Directory.CreateDirectory(curDir);
    }

    // Write data to the file
    using (StreamWriter writer = File.AppendText(Path.Combine(curDir, "data.txt")))
    {
        writer.WriteLine("API0 - " + todo.Data);
    }


    var httpResponse = await httpClient.PostAsync("/todos", new StringContent(data, Encoding.UTF8, "application/json"));
    var pongResponse = await httpResponse.Content.ReadAsStringAsync();

    return pongResponse;
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.Run();
