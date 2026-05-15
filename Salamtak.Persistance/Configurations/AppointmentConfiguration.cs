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
    public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
    {
        public void Configure(EntityTypeBuilder<Appointment> builder)
        {
            builder.ToTable("Appointments");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.PatientId)
                   .IsRequired();

            builder.Property(a => a.DoctorId)
                   .IsRequired();
            builder.Property(a => a.Reason)
                   .HasMaxLength(500)
                   .IsRequired(false); 
            builder.Property(a => a.ClinicId)
                   .IsRequired();

            builder.Property(a => a.AvailabilitySlotId)
                   .IsRequired();

            builder.Property(a => a.Status)
                   .IsRequired();

            builder.Property(a => a.BookingMethod)
                   .IsRequired();

            builder.Property(a => a.BookingCode)
                   .IsRequired()
                   .HasMaxLength(20);

            builder.HasIndex(a => a.BookingCode)
                   .IsUnique();

            builder.Property(a => a.CancelReason)
                   .HasMaxLength(500)
                   .IsRequired(false);

            builder.Property(a => a.CreatedAt)
                   .IsRequired();

            builder.Property(a => a.UpdatedAt)
                   .IsRequired(false);

            builder.Property(a => a.IsDeleted)
                   .IsRequired()
                   .HasDefaultValue(false);

            builder.HasOne(a => a.Patient)
                   .WithMany(p => p.Appointments)
                   .HasForeignKey(a => a.PatientId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.Doctor)
                   .WithMany(d => d.Appointments)
                   .HasForeignKey(a => a.DoctorId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.Clinic)
                   .WithMany(c => c.Appointments)
                   .HasForeignKey(a => a.ClinicId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.AvailabilitySlot)
                   .WithOne(s => s.Appointment)
                   .HasForeignKey<Appointment>(a => a.AvailabilitySlotId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.MedicalReportEntry)
                   .WithOne(e => e.Appointment)
                   .HasForeignKey<MedicalReportEntry>(e => e.AppointmentId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.Feedback)
                   .WithOne(f => f.Appointment)
                   .HasForeignKey<Feedback>(f => f.AppointmentId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(a => a.Notifications)
                   .WithOne(n => n.Appointment)
                   .HasForeignKey(n => n.AppointmentId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(a => a.AvailabilitySlotId)
                   .IsUnique();
        }
    }
}
