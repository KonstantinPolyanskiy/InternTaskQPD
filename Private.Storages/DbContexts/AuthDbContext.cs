using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Private.StorageModels;

namespace Private.Storages.DbContexts;

public class AuthDbContext : IdentityDbContext<ApplicationUserEntity, IdentityRole, string>
{
    public DbSet<BlackListTokenAccessEntity> BlackListTokens => Set<BlackListTokenAccessEntity>();
    public DbSet<RefreshTokenEntity> RefreshTokens => Set<RefreshTokenEntity>();
    public DbSet<ApplicationUserEntity> ApplicationUsers => Set<ApplicationUserEntity>();
    
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) {}

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<RefreshTokenEntity>(e =>
        {
            e.ToTable("refresh_token");
            
            e.HasKey(x => x.Id);
            
            e.Property(x => x.UserId)
                .HasColumnName("user_id")
                .HasComment("FK на таблицу пользователей")
                .IsRequired();

            e.Property(x => x.RefreshTokenBody)
                .HasColumnName("refresh_token_body")
                .HasComment("Тело refresh токена, передаваемое в запросах")
                .IsRequired();
            
            e.Property(x => x.Jti)
                .HasColumnName("jti")
                .HasComment("Jti связанного с этим refresh токеном access токена")
                .IsRequired();
            
            e.Property(x => x.ExpiresAtUtc)
                .HasColumnName("expires_at_utc")
                .HasComment("Через сколько секунд истекает срок валидности refresh токена")
                .IsRequired();
            
            e.HasIndex(x => x.RefreshTokenBody).IsUnique();
            e.HasIndex(x => x.Jti).IsUnique();
            
            e.HasOne(rt => rt.ApplicationUser)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<BlackListTokenAccessEntity>(e =>
        {
            e.ToTable("black_list_token");

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

        builder.Entity<ApplicationUserEntity>(e =>
        {
            e.HasOne(u => u.EmailConfirmationToken)
                .WithOne(t => t.User)
                .HasForeignKey<EmailConfirmationTokenEntity>(t => t.UserId)
                .IsRequired(false)                   
                .OnDelete(DeleteBehavior.Cascade);   
        });
    }
}