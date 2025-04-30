using Car.Dal.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Models.Shared.User;

namespace Car.Dal;

public class AuthDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<BlacklistTokenEntity> BlacklistTokens => Set<BlacklistTokenEntity>();
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) {}

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        
        b.Entity<RefreshToken>(refreshTokenEntity =>
        {
            refreshTokenEntity.ToTable("refresh_tokens");
            refreshTokenEntity.HasKey(x => x.Id);
            
            refreshTokenEntity.Property(x => x.UserId)
                .HasColumnName("user_id")
                .IsRequired();
            
            refreshTokenEntity.Property(x => x.Token)
                .HasColumnName("token")
                .IsRequired();
            
            refreshTokenEntity.Property(x => x.ExpiresAtUtc)
                .HasColumnName("expires_at_utc")
                .IsRequired();
            
            refreshTokenEntity.Property(x => x.Jti)
                .HasColumnName("jti")
                .HasComment("jti access токена, выданный вместе с refresh")
                .IsRequired();
            
            refreshTokenEntity.HasIndex(x => x.Token)
                .IsUnique();
            
            refreshTokenEntity
                .HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<BlacklistTokenEntity>(blackListEntity =>
        {
            blackListEntity.ToTable("blacklist_tokens");
            blackListEntity.HasKey(x => x.Id);

            blackListEntity.Property(x => x.Jti)
                .HasColumnName("jti")
                .IsRequired();

            blackListEntity.Property(x => x.ExpiresAt)
                .HasColumnName("expires_at")
                .IsRequired();

            blackListEntity.HasIndex(x => x.Jti).IsUnique();
        });
    }
}