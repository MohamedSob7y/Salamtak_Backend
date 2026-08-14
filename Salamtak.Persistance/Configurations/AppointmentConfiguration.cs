//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata.Builders;
//using Salamtak.Domain.Models;
//using System;
//using System.Collections.Generic;
//using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Salamtak.Domain.Models;

namespace Salamtak.Persistance.Configurations
{
    public class AppointmentConfiguration
        : IEntityTypeConfiguration<Appointment>
    {
        public void Configure(
            EntityTypeBuilder<Appointment> builder)
        {
            builder.ToTable("Appointments");

            builder.HasKey(appointment =>
                appointment.Id);

            builder.Property(appointment =>
                    appointment.PatientId)
                .IsRequired();

            builder.Property(appointment =>
                    appointment.DoctorId)
                .IsRequired();

            builder.Property(appointment =>
                    appointment.ClinicId)
                .IsRequired();

            builder.Property(appointment =>
                    appointment.AvailabilitySlotId)
                .IsRequired();

            builder.Property(appointment =>
                    appointment.Status)
                .IsRequired();

            builder.Property(appointment =>
                    appointment.BookingMethod)
                .IsRequired();

            builder.Property(appointment =>
                    appointment.BookingCode)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(appointment =>
                    appointment.Reason)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(appointment =>
                    appointment.CancelReason)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(appointment =>
                    appointment.CreatedAt)
                .IsRequired();

            builder.Property(appointment =>
                    appointment.UpdatedAt)
                .IsRequired(false);

            builder.Property(appointment =>
                    appointment.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.HasIndex(appointment =>
                    appointment.BookingCode)
                .IsUnique();

            builder.HasOne(appointment =>
                    appointment.Patient)
                .WithMany(patient =>
                    patient.Appointments)
                .HasForeignKey(appointment =>
                    appointment.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(appointment =>
                    appointment.Doctor)
                .WithMany(doctor =>
                    doctor.Appointments)
                .HasForeignKey(appointment =>
                    appointment.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(appointment =>
                    appointment.Clinic)
                .WithMany(clinic =>
                    clinic.Appointments)
                .HasForeignKey(appointment =>
                    appointment.ClinicId)
                .OnDelete(DeleteBehavior.Restrict);

            /*
             * Slot واحدة يمكن أن يكون لها أكثر من
             * Appointment تاريخية.
             *
             * مثال:
             * Appointment أولى تم إلغاؤها.
             * ثم Appointment ثانية تم إنشاؤها لنفس Slot.
             */
            builder.HasOne(appointment =>
                    appointment.AvailabilitySlot)
                .WithMany(slot =>
                    slot.Appointments)
                .HasForeignKey(appointment =>
                    appointment.AvailabilitySlotId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(appointment =>
                    appointment.MedicalReportEntry)
                .WithOne(entry =>
                    entry.Appointment)
                .HasForeignKey<MedicalReportEntry>(
                    entry => entry.AppointmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(appointment =>
                    appointment.Feedback)
                .WithOne(feedback =>
                    feedback.Appointment)
                .HasForeignKey<Feedback>(
                    feedback => feedback.AppointmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(appointment =>
                    appointment.Notifications)
                .WithOne(notification =>
                    notification.Appointment)
                .HasForeignKey(notification =>
                    notification.AppointmentId)
                .OnDelete(DeleteBehavior.SetNull);


            builder.HasIndex(appointment =>
                    appointment.AvailabilitySlotId)
                .IsUnique()
                .HasFilter(
                    "[IsDeleted] = 0 AND [Status] IN (1, 2)");
        }
    }
}