var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet(
    "/health",
    () => Results.Ok(
        new
        {
            status = "Healthy",
            service = "DriveOS.Api",
            timestamp = DateTimeOffset.UtcNow
        }))
    .WithName("GetHealth")
    .WithTags("System");

app.Run();

public partial class Program;