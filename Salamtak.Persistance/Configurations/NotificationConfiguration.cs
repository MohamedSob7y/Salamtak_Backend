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
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("Notifications");

            builder.HasKey(n => n.Id);

            builder.Property(n => n.UserId)
                   .IsRequired();

            builder.Property(n => n.AppointmentId)
                   .IsRequired(false);

            builder.Property(n => n.Title)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(n => n.Message)
                   .IsRequired()
                   .HasMaxLength(1000);

            builder.Property(n => n.Type)
                   .IsRequired();

            builder.Property(n => n.Channel)
                   .IsRequired();

            builder.Property(n => n.Status)
                   .IsRequired();

            builder.Property(n => n.IsRead)
                   .IsRequired()
                   .HasDefaultValue(false);

            builder.Property(n => n.SentAt)
                   .IsRequired(false);

            builder.Property(n => n.CreatedAt)
                   .IsRequired();

            builder.Property(n => n.UpdatedAt)
                   .IsRequired(false);

            builder.Property(n => n.IsDeleted)
                   .IsRequired()
                   .HasDefaultValue(false);

            builder.HasOne(n => n.User)
                   .WithMany(u => u.Notifications)
                   .HasForeignKey(n => n.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(n => n.Appointment)
                   .WithMany(a => a.Notifications)
                   .HasForeignKey(n => n.AppointmentId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(n => new { n.UserId, n.IsRead });
        }
    }
}
