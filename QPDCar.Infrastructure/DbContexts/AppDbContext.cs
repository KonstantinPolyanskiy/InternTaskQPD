using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using QPDCar.Models.BusinessModels.CarModels;
using QPDCar.Models.BusinessModels.PhotoModels;
using QPDCar.Models.StorageModels;

namespace QPDCar.Infrastructure.DbContexts;

/// <summary> Основной DbContext приложения </summary>
public class AppDbContext : IdentityDbContext<ApplicationUserEntity, IdentityRole, string>
{
    public DbSet<BlackListAccessTokenEntity> BlackListToken => Set<BlackListAccessTokenEntity>();
    public DbSet<RefreshTokenEntity> RefreshToken => Set<RefreshTokenEntity>();
    public DbSet<ApplicationUserEntity> ApplicationUser => Set<ApplicationUserEntity>();
    
    public DbSet<EmailConfirmationTokenEntity> EmailConfirmationToken { get; set; }
    
    public DbSet<PhotoDataEntity> PhotoData { get; set; }
    
    public DbSet<CarEntity> Cars { get; set; }
    
    public DbSet<PhotoMetadataEntity> PhotoMetadata { get; set; }
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        
        b.Entity<RefreshTokenEntity>(e =>
        {
            e.HasKey(x => x.Id);
            
            e.Property(x => x.UserId)
                .HasColumnName("user_id")
                .HasComment("FK на таблицу пользователей")
                .IsRequired();

            e.Property(x => x.RefreshBody)
                .HasColumnName("refresh_token_body")
                .HasComment("Тело refresh токена, передаваемое в запросах")
                .IsRequired();
            
            e.Property(x => x.AccessJti)
                .HasColumnName("jti")
                .HasComment("Jti связанного с этим refresh access токена")
                .IsRequired();
            
            e.Property(x => x.ExpiresAtUtc)
                .HasColumnName("expires_at_utc")
                .HasComment("Через сколько секунд истекает срок валидности refresh токена")
                .IsRequired();
            
            e.HasIndex(x => x.RefreshBody).IsUnique();
            e.HasIndex(x => x.AccessJti).IsUnique();
            
            e.HasOne(rt => rt.ApplicationUser)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<BlackListAccessTokenEntity>(e =>
        {
            e.HasKey(x => x.Id);

            e.Property(x => x.Jti)
                .HasColumnName("jti")
                .HasComment("уникальный jwt id")
                .IsRequired();

            e.Property(x => x.Reason)
                .HasColumnName("reason")
                .HasComment("Почему токен внесен в чс")
                .HasDefaultValue("Unknown")
                .IsRequired();
            
            e.Property(x => x.ExpiresAtUtc)
                .HasColumnName("expires_at_utc")
                .HasComment("Через сколько секунд истекает срок валидности access токена")
                .IsRequired();
            
            e.HasIndex(x => x.Reason).IsUnique();
        });

        b.Entity<ApplicationUserEntity>(e =>
        {
            e.HasOne(u => u.EmailConfirmationToken)
                .WithOne(t => t.User)
                .HasForeignKey<EmailConfirmationTokenEntity>(t => t.UserId)
                .IsRequired(false)                   
                .OnDelete(DeleteBehavior.Cascade);   
        });

        b.Entity<EmailConfirmationTokenEntity>(e =>
        {
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

        b.Entity<PhotoDataEntity>(e =>
        {
            e.HasKey(x => x.Id);
            
            e.Property(x => x.PhotoBytes)
                .HasColumnName("photo_bytes")
                .HasComment("Данные фото (массив байт)")
                .HasColumnType("bytea");
        });

        b.Entity<PhotoMetadataEntity>(e =>
        {
            e.HasKey(x => x.Id);

            e.Property(x => x.Id)
                .HasColumnName("id");

            e.Property(x => x.PhotoDataId)
                .HasColumnName("photo_data_id")
                .HasComment("Guid непосредственно фото. Не FK, т.к. может храниться вне БД");

            e.Property(x => x.StorageType)
                .HasColumnName("storage_type")
                .HasConversion(new EnumToStringConverter<PhotoStorageTypes>())
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
            
            e.Property(c => c.ResponsiveManagerId)
                .HasColumnName("responsive_manager_id")
                .HasComment("Id менеджера ответственного за машину")
                .IsRequired();

            e.Property(c => c.PrioritySale)
                .HasColumnName("priority_sale")
                .HasConversion(new EnumToStringConverter<PrioritySaleTypes>())
                .IsRequired();

            e.Property(c => c.CarCondition)
                .HasColumnName("car_condition")
                .HasConversion(new EnumToStringConverter<ConditionTypes>())
                .IsRequired();

            e.HasOne(c => c.PhotoMetadata)
                .WithOne(pm => pm.Car)
                .HasForeignKey<CarEntity>(c => c.PhotoMetadataId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}