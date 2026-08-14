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
    public class PaymentTransactionConfiguration
        : IEntityTypeConfiguration<PaymentTransaction>
    {
        public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
        {
            builder.ToTable("PaymentTransactions");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.PatientId)
                .IsRequired();

            builder.Property(p => p.AppointmentId)
                .IsRequired();

            builder.Property(p => p.Provider)
                .IsRequired();

            builder.Property(p => p.Status)
                .IsRequired();

            builder.Property(p => p.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(p => p.Currency)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(p => p.PaymobOrderId)
                .HasMaxLength(100)
                .IsRequired(false);

            builder.Property(p => p.PaymobTransactionId)
                .HasMaxLength(100)
                .IsRequired(false);

            builder.Property(p => p.PaymobPaymentKey)
                .HasMaxLength(2000)
                .IsRequired(false);

            builder.Property(p => p.PaymentUrl)
                .HasMaxLength(2000)
                .IsRequired(false);

            builder.Property(p => p.FailureReason)
                .HasMaxLength(1000)
                .IsRequired(false);

            builder.Property(p => p.PaidAt)
                .IsRequired(false);

            builder.Property(p => p.CreatedAt)
                .IsRequired();

            builder.Property(p => p.UpdatedAt)
                .IsRequired(false);

            builder.Property(p => p.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.HasOne(p => p.Patient)
                .WithMany()
                .HasForeignKey(p => p.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.Appointment)
                .WithMany()
                .HasForeignKey(p => p.AppointmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(p => p.AppointmentId);

            builder.HasIndex(p => p.PatientId);

            builder.HasIndex(p => p.PaymobOrderId);

            builder.HasIndex(p => p.PaymobTransactionId);
        }
    }
}
