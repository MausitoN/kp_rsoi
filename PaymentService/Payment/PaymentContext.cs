using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PaymentService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentService
{
    public class PaymentContext : DbContext
    {
        public DbSet<Payments> Payments { get; set; }

        public PaymentContext(DbContextOptions options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            builder.EnableSensitiveDataLogging();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new PaymentsConfiguration());
        }

        public class PaymentsConfiguration : IEntityTypeConfiguration<Payments>
        {
            public void Configure(EntityTypeBuilder<Payments> builder)
            {
                builder.ToTable("Payments");
                builder.Property(p => p.Id).HasColumnType("UUID");

                builder.Property(p => p.Status).HasConversion(new EnumToStringConverter<PaymentStatus>()).IsRequired();
            }
        }
    }
}
