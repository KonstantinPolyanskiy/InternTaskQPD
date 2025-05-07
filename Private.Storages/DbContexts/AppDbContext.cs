using Microsoft.EntityFrameworkCore;
using Private.StorageModels;

namespace Private.Storages.DbContexts;

public class AppDbContext : DbContext
{
    public DbSet<EmailConfirmationTokenEntity> EmailConfirmationTokens { get; set; }
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<EmailConfirmationTokenEntity>(e =>
        {
            e.ToTable("email_confirmation_token");
            
            e.HasKey(x => x.Id);
            
            e.Property(x => x.TokenBody)
                .HasComment("тело токена")
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
    }
}