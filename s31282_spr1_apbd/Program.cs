var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

//builder.Services.AddScoped<IWarehouseService, WarehouseService>();

var app = builder.Build();



app.UseAuthorization();

app.MapControllers();

app.Run();