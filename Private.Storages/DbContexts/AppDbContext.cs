using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Private.StorageModels;
using Public.Models.BusinessModels.CarModels;
using Public.Models.BusinessModels.StorageModels;

namespace Private.Storages.DbContexts;

public class AppDbContext : DbContext
{
    public DbSet<EmailConfirmationTokenEntity> EmailConfirmationTokens { get; set; }
    
    public DbSet<PhotoEntity> Photos { get; set; }
    
    public DbSet<CarEntity> Cars { get; set; }
    
    public DbSet<PhotoMetadataEntity> PhotoMetadatas { get; set; }
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<EmailConfirmationTokenEntity>(e =>
        {
            e.ToTable("email_confirmation_token");
            
            e.HasKey(x => x.Id);
            
            e.Property(x => x.TokenBody)
                .HasComment("Тело токена")
                .IsRequired();
            
            e.Property(x => x.ExpiresAt)
                .IsRequired();
            
            e.Property(x => x.Confirmed)
                .HasDefaultValue(false)
                .IsRequired();
            
            e.Property(x => x.ConfirmedAt)
                .IsRequired(false);

            e.HasIndex(x => x.TokenBody).IsUnique();
        });

        b.Entity<PhotoEntity>(e =>
        {
            e.ToTable("photo");
            
            e.HasKey(x => x.Id);
            
            e.Property(x => x.PhotoBytes)
                .HasColumnName("photo_bytes")
                .HasComment("Данные фото (массив байт)")
                .HasColumnType("bytea");
        });

        b.Entity<PhotoMetadataEntity>(e =>
        {
            e.ToTable("photo_metadata");

            e.HasKey(x => x.Id);

            e.Property(x => x.Id)
                .HasColumnName("id");

            e.Property(x => x.PhotoDataId)
                .HasColumnName("photo_data_id")
                .HasComment("Guid непосредственно фото. Не FK, т.к. может храниться вне БД");

            e.Property(x => x.StorageType)
                .HasColumnName("storage_type")
                .HasConversion(new EnumToStringConverter<StorageTypes>())
                .IsRequired();

            e.Property(x => x.Extension)
                .HasColumnName("extension")
                .HasConversion(new EnumToStringConverter<ImageFileExtensions>())
                .IsRequired();

            e.Property(x => x.CarId)
                .HasColumnName("car_id")
                .IsRequired();

            e.HasIndex(x => x.CarId)
                .IsUnique();
        });

        b.Entity<CarEntity>(e =>
        {
            e.ToTable("cars");

            e.HasKey(c => c.Id);

            e.Property(c => c.Id)
                .HasColumnName("id");

            e.Property(c => c.Brand)
                .HasColumnName("brand")
                .HasMaxLength(100)
                .IsRequired();

            e.Property(c => c.Color)
                .HasColumnName("color")
                .HasMaxLength(50)
                .IsRequired();

            e.Property(c => c.Price)
                .HasColumnName("price")
                .HasColumnType("numeric(18,2)")
                .IsRequired();

            e.Property(c => c.CurrentOwner)
                .HasColumnName("current_owner")
                .HasMaxLength(100);

            e.Property(c => c.Mileage)
                .HasColumnName("mileage");

            e.Property(c => c.PrioritySale)
                .HasColumnName("priority_sale")
                .HasConversion(new EnumToStringConverter<PrioritySaleTypes>())
                .IsRequired();

            e.Property(c => c.CarCondition)
                .HasColumnName("car_condition")
                .HasConversion(new EnumToStringConverter<CarConditionTypes>())
                .IsRequired();

            e.HasOne(c => c.PhotoMetadata)
                .WithOne(pm => pm.Car)
                .HasForeignKey<CarEntity>(c => c.PhotoMetadataId)
                .OnDelete(DeleteBehavior.SetNull);  
        });
    }
}