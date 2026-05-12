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
    public class AvailabilitySlotConfiguration : IEntityTypeConfiguration<AvailabilitySlot>
    {
        public void Configure(EntityTypeBuilder<AvailabilitySlot> builder)
        {
            builder.ToTable("AvailabilitySlots");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.DoctorId)
                   .IsRequired();

            builder.Property(s => s.ClinicId)
                   .IsRequired();

            builder.Property(s => s.StartTime)
                   .IsRequired();

            builder.Property(s => s.EndTime)
                   .IsRequired();

            builder.Property(s => s.IsAvailable)
                   .IsRequired()
                   .HasDefaultValue(true);

            builder.Property(s => s.CreatedAt)
                   .IsRequired();

            builder.Property(s => s.UpdatedAt)
                   .IsRequired(false);

            builder.Property(s => s.IsDeleted)
                   .IsRequired()
                   .HasDefaultValue(false);

            builder.HasOne(s => s.Doctor)
                   .WithMany(d => d.AvailabilitySlots)
                   .HasForeignKey(s => s.DoctorId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.Clinic)
                   .WithMany(c => c.AvailabilitySlots)
                   .HasForeignKey(s => s.ClinicId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.Appointment)
                   .WithOne(a => a.AvailabilitySlot)
                   .HasForeignKey<Appointment>(a => a.AvailabilitySlotId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(s => new { s.DoctorId, s.StartTime, s.EndTime })
                   .IsUnique();

            builder.HasCheckConstraint(
                "CK_AvailabilitySlot_EndTime_After_StartTime",
                "[EndTime] > [StartTime]"
            );
        }
    }
}
