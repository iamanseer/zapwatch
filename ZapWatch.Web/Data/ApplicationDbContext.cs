using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ZapWatch.Web.Models;

namespace ZapWatch.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options), IDataProtectionKeyContext
{
    public DbSet<Automation> Automations { get; set; } = null!;
    public DbSet<PingEvent> PingEvents { get; set; } = null!;
    public DbSet<AlertEvent> AlertEvents { get; set; } = null!;
    public DbSet<Microsoft.AspNetCore.DataProtection.EntityFrameworkCore.DataProtectionKey> DataProtectionKeys { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Automation>()
            .HasIndex(a => a.PingToken)
            .IsUnique();

        builder.Entity<Automation>()
            .HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<PingEvent>()
            .HasOne(p => p.Automation)
            .WithMany(a => a.PingEvents)
            .HasForeignKey(p => p.AutomationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<AlertEvent>()
            .HasOne(a => a.Automation)
            .WithMany(a => a.AlertEvents)
            .HasForeignKey(a => a.AutomationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
