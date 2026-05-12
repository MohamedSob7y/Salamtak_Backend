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
    public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
    {
        public void Configure(EntityTypeBuilder<Doctor> builder)
        {
            builder.ToTable("Doctors");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.UserId)
                   .IsRequired();

            builder.HasIndex(d => d.UserId)
                   .IsUnique();

            builder.Property(d => d.SpecialtyId)
                   .IsRequired();

            builder.Property(d => d.Bio)
                   .HasMaxLength(1000);

            builder.Property(d => d.ExperienceYears)
                   .IsRequired();

            builder.Property(d => d.LicenseNumber)
                   .HasMaxLength(100);

            builder.Property(d => d.VerificationStatus)
                   .IsRequired();

            builder.Property(d => d.IsVerified)
                   .IsRequired()
                   .HasDefaultValue(false);

            builder.Property(d => d.AverageRating)
                   .IsRequired()
                   .HasDefaultValue(0);

            builder.Property(d => d.CreatedAt)
                   .IsRequired();

            builder.Property(d => d.UpdatedAt)
                   .IsRequired(false);

            builder.Property(d => d.IsDeleted)
                   .IsRequired()
                   .HasDefaultValue(false);

            builder.HasOne(d => d.User)
                   .WithOne(u => u.Doctor)
                   .HasForeignKey<Doctor>(d => d.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(d => d.Specialty)
                   .WithMany(s => s.Doctors)
                   .HasForeignKey(d => d.SpecialtyId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(d => d.Clinics)
                   .WithOne(c => c.Doctor)
                   .HasForeignKey(c => c.DoctorId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(d => d.AvailabilitySlots)
                   .WithOne(s => s.Doctor)
                   .HasForeignKey(s => s.DoctorId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(d => d.Appointments)
                   .WithOne(a => a.Doctor)
                   .HasForeignKey(a => a.DoctorId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(d => d.MedicalReportEntries)
                   .WithOne(e => e.Doctor)
                   .HasForeignKey(e => e.DoctorId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(d => d.DoctorDocuments)
                   .WithOne(doc => doc.Doctor)
                   .HasForeignKey(doc => doc.DoctorId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(d => d.Feedbacks)
                   .WithOne(f => f.Doctor)
                   .HasForeignKey(f => f.DoctorId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
