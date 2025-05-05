using Backend.Dal.Models;
using Enum.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Backend.Dal;

public class AppDbContext : DbContext
{
    public DbSet<CarEntity> Cars => Set<CarEntity>();
    public DbSet<PhotoEntity> Photos => Set<PhotoEntity>();

    public DbSet<PhotoMetadataEntity> PhotosMetadata => Set<PhotoMetadataEntity>();
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<CarEntity>(carEntity =>
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

            carEntity.Property(e => e.CurrentOwner)
                .HasColumnName("current_owner");

            carEntity.Property(e => e.Mileage)
                .HasColumnName("mileage");
            
            carEntity.Property(c => c.CarCondition)
                .HasColumnName("car_condition")
                .HasConversion(new EnumToStringConverter<CarCondition>())
                .HasComment("Car condition");
            
            carEntity.Property(c => c.PrioritySale)
                .HasColumnName("priority_sale")
                .HasConversion(new EnumToStringConverter<PrioritySale>())
                .HasComment("Car priority sale");
            
            carEntity.Property(e => e.PhotoMetadataId)
                .HasColumnName("photo_metadata_id")
                .IsRequired(false);
            
            carEntity.HasOne(c => c.PhotoMetadata)
                .WithOne(m => m.Car)
                .HasForeignKey<CarEntity>(c => c.PhotoMetadataId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_car_photo_metadata");
        });

        b.Entity<PhotoEntity>(photoEntity =>
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
            
        });

        b.Entity<PhotoMetadataEntity>(metadataEntity =>
        {
            metadataEntity.ToTable("photo_metadata");

            metadataEntity.HasKey(e => e.Id);
            metadataEntity.Property(e => e.Id).HasColumnName("id");

            metadataEntity.Property(e => e.StorageType)
                .HasColumnName("storage_type")
                .HasConversion(new EnumToStringConverter<PhotoStorageType>())
                .IsRequired();
            
            metadataEntity.Property(e => e.PhotoId)
                .HasColumnName("photo_id")
                .IsRequired(false);

            metadataEntity.Property(e => e.Extension)
                .HasColumnName("extension")
                .HasConversion(new EnumToStringConverter<PhotoFileExtension>())
                .IsRequired();
        });
    }
}