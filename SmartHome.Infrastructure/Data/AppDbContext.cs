using Microsoft.EntityFrameworkCore;
using SmartHome.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SmartHome.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Device> Devices => Set<Device>();
        public DbSet<DeviceReading> DeviceReadings => Set<DeviceReading>();
        public DbSet<AutomationRule> Automations => Set<AutomationRule>();
        public DbSet<EnergyLog> EnergyLogs => Set<EnergyLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.FullName).HasMaxLength(100).IsRequired();
                entity.Property(x => x.Email).HasMaxLength(150).IsRequired();
                entity.Property(x => x.PasswordHash).HasMaxLength(255).IsRequired();
                entity.Property(x => x.Role).HasMaxLength(50).IsRequired();
                entity.Property(x => x.CreatedAt).HasColumnType("datetime2");

                entity.HasIndex(x => x.Email).IsUnique();
            });

            modelBuilder.Entity<Device>(entity =>
            {
                entity.ToTable("Devices");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
                entity.Property(x => x.Location).HasMaxLength(100);
                entity.Property(x => x.PowerConsumption).HasColumnType("float");
                entity.Property(x => x.CreatedAt).HasColumnType("datetime2");

                entity.HasOne(x => x.User)
                      .WithMany(x => x.Devices)
                      .HasForeignKey(x => x.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<DeviceReading>(entity =>
            {
                entity.ToTable("DeviceReadings");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Value).HasColumnType("float");
                entity.Property(x => x.Timestamp).HasColumnType("datetime2");

                entity.HasOne(x => x.Device)
                      .WithMany(x => x.Readings)
                      .HasForeignKey(x => x.DeviceId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<AutomationRule>(entity =>
            {
                entity.ToTable("Automations");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
                entity.Property(x => x.ConditionValue).HasColumnType("float");
                entity.Property(x => x.CreatedAt).HasColumnType("datetime2");

                entity.HasOne(x => x.User)
                      .WithMany(x => x.Automations)
                      .HasForeignKey(x => x.UserId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(x => x.TargetDevice)
                      .WithMany(x => x.AutomationRules)
                      .HasForeignKey(x => x.TargetDeviceId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<EnergyLog>(entity =>
            {
                entity.ToTable("EnergyLogs");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.ConsumedWatts).HasColumnType("float");
                entity.Property(x => x.RecordedAt).HasColumnType("datetime2");

                entity.HasOne(x => x.Device)
                      .WithMany(x => x.EnergyLogs)
                      .HasForeignKey(x => x.DeviceId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}