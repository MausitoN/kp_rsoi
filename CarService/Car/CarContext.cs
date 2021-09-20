using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CarService.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CarService
{
    public class CarContext : DbContext
    {
        public DbSet<Automobile> Cars { get; set; }

        public CarContext(DbContextOptions options) : base(options)
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

    public class CarsConfiguration : IEntityTypeConfiguration<Automobile>
    {
        public void Configure(EntityTypeBuilder<Automobile> builder)
        {
            builder.ToTable("Automobile");

            builder.Property(a => a.Id).HasColumnType("UUID");

            builder.Property(a => a.CarType).HasConversion(new EnumToStringConverter<CarType>()).IsRequired();
        }
    }
}
