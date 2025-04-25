using AutoMapper;
using Car.Dal.Repository;
using Car.Dal.Repository.EntityFrameworkRepository;
using Car.App.Extensions;
using Car.App.Profiles;
using Car.App.Services.Repositories;
using Car.Dal;
using CarService.Profiles;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// TODO: логгер

// Data Layer
builder.Services.AddDbContext<AppDbContext>((sp, opt) =>
{
    var conn = "Host=localhost;Port=5313;Database=main_db;Username=admin;Password=password";

    opt.UseNpgsql(conn, npg =>
    {
        npg.EnableRetryOnFailure();                               
    });
});

builder.Services.AddScoped<ICarRepository, PostgresCarRepository>();
builder.Services.AddScoped<IPhotoRepository, PostgresPhotoRepository>();

// Domain Layer
builder.Services.AddAutoMapper(typeof(CarProfileForApi).Assembly);
builder.Services.AddAutoMapper(typeof(CarProfileForApp).Assembly);
builder.Services.AddApp();

// Api Layer
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Build application
var app = builder.Build();

// Migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();                 
}

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

app.Run();