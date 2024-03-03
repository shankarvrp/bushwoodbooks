using Serilog;
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

var pongAPI = builder.Configuration.GetConnectionString("PongAPIURL");
builder.Services.AddHttpClient("PongClient", c => { c.BaseAddress = new Uri(pongAPI); });

var app = builder.Build();


app.MapGet("/", (ILogger logger) =>
{
    logger.Information("Pong's ping!");
    return "Pong live!";
});

app.MapPost("/send", async  (ILogger logger, HttpRequest request) =>
{
    var memStream = new MemoryStream();
    await request.Body.CopyToAsync(memStream);
    memStream.Seek(0, SeekOrigin.Begin);
    var dataToWrite = await new StreamReader(memStream).ReadToEndAsync();

    dataToWrite = "Pong response - " + dataToWrite    ;
    logger.Information(dataToWrite);

    var appPath = builder.Environment.ContentRootPath;
    var dataPath = Path.Combine(appPath, "data/data.txt");
    
    System.IO.File.AppendAllLines(dataPath, new string[] { dataToWrite + "  ::  " + DateTime.Now.ToString() });

    return dataToWrite;
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
