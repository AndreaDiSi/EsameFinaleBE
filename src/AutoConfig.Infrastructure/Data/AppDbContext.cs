using AutoConfig.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoConfig.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<CarModel> CarModels => Set<CarModel>();
    public DbSet<Motorization> Motorizations => Set<Motorization>();
    public DbSet<CarOption> CarOptions => Set<CarOption>();
    public DbSet<VehicleConfiguration> Configurations => Set<VehicleConfiguration>();
    public DbSet<Quote> Quotes => Set<Quote>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Role).HasConversion<string>();
        });

        mb.Entity<CarModel>(e =>
            e.Property(m => m.Category).HasConversion<string>());

        mb.Entity<Motorization>(e =>
            e.Property(m => m.FuelType).HasConversion<string>());

        mb.Entity<CarOption>(e =>
        {
            e.Property(o => o.Category).HasConversion<string>();

            e.HasMany(o => o.IncompatibleWith)
             .WithMany(o => o.IncompatibleWithMe)
             .UsingEntity(j => j.ToTable("OptionIncompatibilities"));

            e.HasMany(o => o.RequiredMotorizations)
             .WithMany(m => m.RequiredByOptions)
             .UsingEntity(j => j.ToTable("OptionMotorizationRequirements"));
        });

        mb.Entity<VehicleConfiguration>(e =>
        {
            e.HasMany(c => c.Options)
             .WithMany(o => o.Configurations)
             .UsingEntity(j => j.ToTable("ConfigurationOptions"));

            e.HasOne(c => c.User).WithMany(u => u.Configurations)
             .HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Cascade);

            e.HasOne(c => c.Model).WithMany(m => m.Configurations)
             .HasForeignKey(c => c.ModelId).OnDelete(DeleteBehavior.Restrict);

            e.HasOne(c => c.Motorization).WithMany(m => m.Configurations)
             .HasForeignKey(c => c.MotorizationId).OnDelete(DeleteBehavior.Restrict);
        });

        mb.Entity<Quote>(e =>
        {
            e.Property(q => q.Status).HasConversion<string>();

            e.HasOne(q => q.Configuration).WithMany(c => c.Quotes)
             .HasForeignKey(q => q.ConfigurationId).OnDelete(DeleteBehavior.Cascade);

            e.HasOne(q => q.User).WithMany(u => u.Quotes)
             .HasForeignKey(q => q.UserId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
