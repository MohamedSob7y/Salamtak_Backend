using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Salamtak.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.Persistance.Configurations
{
    public class ClinicConfiguration : IEntityTypeConfiguration<Clinic>
    {
        public void Configure(EntityTypeBuilder<Clinic> builder)
        {
            builder.ToTable("Clinics");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.DoctorId)
                   .IsRequired();

            builder.Property(c => c.Name)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(c => c.Address)
                   .IsRequired()
                   .HasMaxLength(250);

            builder.Property(c => c.City)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(c => c.PhoneNumber)
                   .HasMaxLength(20);

            builder.Property(c => c.Latitude)
                   .HasColumnType("decimal(9,6)")
                   .IsRequired(false);

            builder.Property(c => c.Longitude)
                   .HasColumnType("decimal(9,6)")
                   .IsRequired(false);

            builder.Property(c => c.CreatedAt)
                   .IsRequired();

            builder.Property(c => c.UpdatedAt)
                   .IsRequired(false);

            builder.Property(c => c.IsDeleted)
                   .IsRequired()
                   .HasDefaultValue(false);

            builder.HasOne(c => c.Doctor)
                   .WithMany(d => d.Clinics)
                   .HasForeignKey(c => c.DoctorId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.AvailabilitySlots)
                   .WithOne(s => s.Clinic)
                   .HasForeignKey(s => s.ClinicId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(c => c.Appointments)
                   .WithOne(a => a.Clinic)
                   .HasForeignKey(a => a.ClinicId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
