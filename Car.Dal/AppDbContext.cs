using System.Security.Cryptography;
using System.Text.Json;
using Car.App.Models.CarModels;
using Car.Dal.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Car.Dal;

public class AppDbContext : DbContext
{
    public DbSet<Models.Car> Cars => Set<Models.Car>();
    public DbSet<Photo> Photos => Set<Photo>();
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<Models.Car>(carEntity =>
        {
            carEntity.ToTable("cars");
            
            carEntity.HasKey(c => c.Id);
            carEntity.Property(c => c.Id)
                .HasColumnName("id")
                .HasComment("Primary key");
            
            carEntity.Property(e => e.Brand)
                .HasColumnName("brand")
                .IsRequired();

            carEntity.Property(e => e.Color)
                .HasColumnName("color")
                .IsRequired();

            carEntity.Property(e => e.Price)
                .HasColumnName("price")
                .IsRequired();
            
            carEntity.Property(c => c.CarCondition)
                .HasColumnName("car_condition")
                .HasConversion(new EnumToStringConverter<CarCondition>())
                .HasComment("Car condition");
            
            carEntity.Property(c => c.PrioritySale)
                .HasColumnName("priority_sale")
                .HasConversion(new EnumToStringConverter<CarPrioritySale>())
                .HasComment("Car priority sale");
            
            carEntity.Property(e => e.PhotoId)
                .HasColumnName("photo_id")
                .IsRequired(false);
            
            carEntity.HasOne(e => e.Photo)
                .WithOne(p => p.Car)
                .HasForeignKey<Models.Car>(c => c.PhotoId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_car_photo");

            // JSONB-поле для любых дополнительных деталей
            carEntity.Property(e => e.Details)
                .HasColumnName("car_details")
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null) ??
                         new()
                );
        
            b.Entity<Photo>(photoEntity =>
            {
                photoEntity.ToTable("photos");

                // PK
                photoEntity.HasKey(e => e.Id);
                photoEntity.Property(e => e.Id)
                    .HasColumnName("id");

                // Содержимое фото
                photoEntity.Property(e => e.PhotoBytes)
                    .HasColumnName("photo_bytes")
                    .HasColumnType("bytea");

                // Расширение
                photoEntity.Property(e => e.Extension)
                    .HasColumnName("extension")
                    .HasMaxLength(20);
            });
        });
    }
}