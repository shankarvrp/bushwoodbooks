using API1;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.ConfigureHttpJsonOptions(options => {
    options.SerializerOptions.WriteIndented = true;
    options.SerializerOptions.IncludeFields = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/hello", ([FromQuery] string name) =>
{
    return $"Hello {name}";
});

app.MapPost("/todos", (Message msg) =>
{
    var result = msg.Data + " - return"; // await Task.FromResult(request.Form["Data"]); //get value from form.

    //Write to the shared data file
    //var appPath = builder.Environment.ContentRootPath;
    //var dataPath = Path.Combine(appPath, "data/data.txt");
    //System.IO.File.AppendAllLines(dataPath, new string[] { result + "  ::  " + DateTime.Now.ToString() });

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
        writer.WriteLine("API1 - " + result);
    }

    return result;
});

app.UseAuthorization();

app.MapControllers();

app.Run();
