using Car.Dal.Models;
using Microsoft.EntityFrameworkCore;

namespace Car.Dal;

public class AppDbContext : DbContext
{
    public DbSet<Models.Car> Cars => Set<Models.Car>();
    public DbSet<Photo> Photos => Set<Photo>();
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Models.Car>(e =>
        {
            e.ToTable("cars");
            e.HasKey(x => x.Id);

            e.HasOne(c => c.Photo)
                .WithOne(p => p.Car)
                .HasForeignKey<Models.Car>(c => c.PhotoId)      
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_car_photo");
        });

        b.Entity<Photo>(e =>
        {
            e.ToTable("photos");
            e.HasKey(x => x.Id);
            
            e.Property(x => x.PhotoBytes).HasColumnType("bytea");
        });
    }
}