using System.Reflection;
using CarService.Daos.CarDao;
using CarService.Daos.CarDao.EntityCarDao;
using CarService.Models.Car.Mapping;
using CarService.Services.CarService;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Microsoft.EntityFrameworkCore.Design;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration));



 

builder.Services.AddScoped<ICarDao, EntityCarDao>();
builder.Services.AddScoped<ICarService, CarService.Services.CarService.CarService>();   

builder.Services.AddAutoMapper(typeof(CarMappingProfile));

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CarDbContext>();
    db.Database.Migrate();
}

app.UseCors("DefaultPolicy");

app.MapControllers();

app.Run();