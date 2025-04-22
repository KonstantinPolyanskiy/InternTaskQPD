using CarService.Models.Car;
using Microsoft.EntityFrameworkCore;

namespace CarService.Daos.CarDao.EntityCarDao;

public class CarDbContext : DbContext
{
    public CarDbContext(DbContextOptions<CarDbContext> options) : base(options) {}
    
    public DbSet<BaseCar> Cars { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BaseCar>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(c => c.Brand).IsRequired();
            entity.Property(c => c.Color).IsRequired();
            entity.Property(c => c.Photo).IsRequired();
            entity.Property(c => c.Price).IsRequired();
        });
        
        modelBuilder.Entity<SecondHandCar>().HasBaseType<BaseCar>();

        modelBuilder.Entity<SecondHandCar>(entity =>
        {
            entity.Property(e => e.Mileage).IsRequired();
            entity.Property(c => c.CurrentOwner).IsRequired();
        });
    }
}