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
    public class MedicalReportEntryConfiguration : IEntityTypeConfiguration<MedicalReportEntry>
    {
        public void Configure(EntityTypeBuilder<MedicalReportEntry> builder)
        {
            builder.ToTable("MedicalReportEntries");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.MedicalReportId)
                   .IsRequired();

            builder.Property(e => e.AppointmentId)
                   .IsRequired();

            builder.Property(e => e.DoctorId)
                   .IsRequired();

            builder.Property(e => e.Diagnosis)
                   .HasMaxLength(1000)
                   .IsRequired(false);

            builder.Property(e => e.Recommendations)
                   .HasMaxLength(1500)
                   .IsRequired(false);

            builder.Property(e => e.Notes)
                   .HasMaxLength(1500)
                   .IsRequired(false);

            builder.Property(e => e.CreatedAt)
                   .IsRequired();

            builder.Property(e => e.UpdatedAt)
                   .IsRequired(false);

            builder.Property(e => e.IsDeleted)
                   .IsRequired()
                   .HasDefaultValue(false);

            builder.HasOne(e => e.MedicalReport)
                   .WithMany(m => m.Entries)
                   .HasForeignKey(e => e.MedicalReportId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.Appointment)
                   .WithOne(a => a.MedicalReportEntry)
                   .HasForeignKey<MedicalReportEntry>(e => e.AppointmentId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Doctor)
                   .WithMany(d => d.MedicalReportEntries)
                   .HasForeignKey(e => e.DoctorId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(e => e.Prescriptions)
                   .WithOne(p => p.MedicalReportEntry)
                   .HasForeignKey(p => p.MedicalReportEntryId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(e => e.AppointmentId)
                   .IsUnique();
        }
    }
}
