using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Text;
using ILogger = Serilog.ILogger;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Logging.ClearProviders();
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

var pongAPI = builder.Configuration.GetValue<string>("PongAPIURL");
builder.Services.AddHttpClient("PongClient", c => { c.BaseAddress = new Uri(pongAPI); });

var app = builder.Build();

app.MapGet("/", (ILogger logger) =>
{
    logger.Information("Ping's ping!");
    return "Ping live!";
});

app.MapPost("/send", async (ILogger logger, [FromBody]string data, IHttpClientFactory httpFactory) =>
{
    string dataToWrite = data;

    var httpClient = httpFactory.CreateClient("PongClient");
    var httpResponse = await httpClient.PostAsync("/send", new StringContent(dataToWrite, Encoding.UTF8, "text/plain"));
    var pongResponse = await httpResponse.Content.ReadAsStringAsync();

    var appPath = builder.Environment.ContentRootPath;
    var dataPath = Path.Combine(appPath, "data/data.txt");

    System.IO.File.AppendAllLines(dataPath, new string[] { dataToWrite + "  ::  " + DateTime.Now.ToString() });

    var finalResponse = "Ping response - " + pongResponse;
    logger.Information(finalResponse);

    return finalResponse;
});


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();


app.UseAuthorization();

app.MapControllers();

app.Run();


