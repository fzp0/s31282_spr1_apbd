using s31282_spr1_apbd.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Services.AddScoped<IVisitService, VisitService>();

var app = builder.Build();



app.UseAuthorization();

app.MapControllers();

app.Run();