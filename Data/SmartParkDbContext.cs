using Microsoft.EntityFrameworkCore;
using SmartPark.Models;

namespace SmartPark.Data;

public class SmartParkDbContext : DbContext
{
    public DbSet<ParkingSlot> ParkingSlots { get; set; }
    public DbSet<ParkingRecord> ParkingRecords { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<AppUser> AppUsers { get; set; }

    public SmartParkDbContext(DbContextOptions<SmartParkDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ParkingSlot>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SlotNumber).IsRequired().HasMaxLength(10);
            entity.HasIndex(e => e.SlotNumber).IsUnique();
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Plate).IsRequired().HasMaxLength(20);
            entity.Property(e => e.PlateNormalized).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => e.PlateNormalized).IsUnique();
            entity.Property(e => e.OwnerName).HasMaxLength(100);
            entity.Property(e => e.RowVersion).IsRowVersion();
        });

        modelBuilder.Entity<ParkingRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.VehicleNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.OwnerName).IsRequired().HasMaxLength(100);
            entity.HasOne(e => e.ParkingSlot)
                  .WithMany()
                  .HasForeignKey(e => e.ParkingSlotId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Role).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });
    }
}
