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
    public class FeedbackConfiguration : IEntityTypeConfiguration<Feedback>
    {
        public void Configure(EntityTypeBuilder<Feedback> builder)
        {
            builder.ToTable("Feedbacks");

            builder.HasKey(f => f.Id);

            builder.Property(f => f.PatientId)
                   .IsRequired();

            builder.Property(f => f.DoctorId)
                   .IsRequired();

            builder.Property(f => f.AppointmentId)
                   .IsRequired();

            builder.Property(f => f.Rating)
                   .IsRequired();

            builder.Property(f => f.Comment)
                   .HasMaxLength(1000)
                   .IsRequired(false);

            builder.Property(f => f.CreatedAt)
                   .IsRequired();

            builder.Property(f => f.UpdatedAt)
                   .IsRequired(false);

            builder.Property(f => f.IsDeleted)
                   .IsRequired()
                   .HasDefaultValue(false);

            builder.HasOne(f => f.Patient)
                   .WithMany(p => p.Feedbacks)
                   .HasForeignKey(f => f.PatientId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(f => f.Doctor)
                   .WithMany(d => d.Feedbacks)
                   .HasForeignKey(f => f.DoctorId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(f => f.Appointment)
                   .WithOne(a => a.Feedback)
                   .HasForeignKey<Feedback>(f => f.AppointmentId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(f => f.AppointmentId)
                   .IsUnique();

            builder.HasCheckConstraint(
                "CK_Feedback_Rating_Range",
                "[Rating] >= 1 AND [Rating] <= 5"
            );
        }
    }
}
