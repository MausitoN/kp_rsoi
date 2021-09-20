using BookingService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;

namespace CarBookingSystem
{
    public class BookingContext : DbContext
    {
        public DbSet<Booking> Bookings { get; set; }

        public BookingContext(DbContextOptions options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            builder.EnableSensitiveDataLogging();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new CarsConfiguration());
        }
    }

    public class CarsConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.ToTable("Booking");

            builder.Property(b => b.CarUid).HasColumnType("UUID");
            builder.Property(b => b.UserUid).HasColumnType("UUID");
            builder.Property(b => b.PaymentUid).HasColumnType("UUID");
            builder.Property(b => b.TakeFromOfficeUid).HasColumnType("UUID");
            builder.Property(b => b.ReturnToOfficeUid).HasColumnType("UUID");

            builder.Property(b => b.Status).HasConversion(new EnumToStringConverter<BookingStatus>()).IsRequired();
        }
    }
}
