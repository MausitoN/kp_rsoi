using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OfficeService.Models;
using System;

namespace Office
{
    public class OfficeContext : DbContext
    {

        public DbSet<RentOffice> RentOffices { get; set; }
        public DbSet<AvailableCar> AvailableCars { get; set; }

        public OfficeContext(DbContextOptions options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            builder.EnableSensitiveDataLogging();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new OfficeConfiguration());
            modelBuilder.ApplyConfiguration(new AvailableCarsConfiguration());
        }
    }

    public class OfficeConfiguration : IEntityTypeConfiguration<RentOffice>
    {
        public void Configure(EntityTypeBuilder<RentOffice> builder)
        {
            builder.ToTable("RentOffices");
            builder
                .HasMany(ro => ro.AvailableCars)
                .WithOne(ac => ac.RentOffice)
                .HasForeignKey(ac => ac.OfficeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasData(RentOffices());
            builder.Property(ro => ro.Id).HasColumnType("UUID");
        }

        private RentOffice[] RentOffices() => new[]
        {
            new RentOffice
            {
                Id = new Guid("3ebf38ad-1ea2-4c46-8f27-c5381b1fa416"),
                Location = "m. Otradnoye"
            },

            new RentOffice
            {
                Id = new Guid("c186c598-b82a-4a95-a176-bab0aebaf67c"),
                Location = "m. Chekhov"
            }
        };
    }

    public class AvailableCarsConfiguration : IEntityTypeConfiguration<AvailableCar>
    {
        public void Configure(EntityTypeBuilder<AvailableCar> builder)
        {
            builder.ToTable("AvailableCars");
            //builder.HasNoKey();
            builder.Property(ac => ac.CarUid).HasColumnType("UUID");
        }
    }
}