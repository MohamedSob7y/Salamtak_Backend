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
    public class DoctorDocumentConfiguration
        : IEntityTypeConfiguration<DoctorDocument>
    {
        public void Configure(
            EntityTypeBuilder<DoctorDocument> builder)
        {
            builder.ToTable("DoctorDocuments");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.DoctorId)
                .IsRequired();

            builder.Property(d => d.VerifiedByAdminId)
                .IsRequired(false);

            builder.Property(d => d.DocumentType)
                .IsRequired();

            builder.Property(d => d.OriginalFileName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(d => d.StoredFileName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(d => d.StoragePath)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(d => d.ContentType)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(d => d.FileSize)
                .IsRequired();

            builder.Property(d => d.Description)
                .IsRequired(false)
                .HasMaxLength(500);

            builder.Property(d => d.IsVerified)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(d => d.RejectionReason)
                .IsRequired(false)
                .HasMaxLength(500);

            builder.Property(d => d.VerifiedAt)
                .IsRequired(false);

            builder.Property(d => d.CreatedAt)
                .IsRequired();

            builder.Property(d => d.UpdatedAt)
                .IsRequired(false);

            builder.Property(d => d.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.HasOne(d => d.Doctor)
                .WithMany(d => d.DoctorDocuments)
                .HasForeignKey(d => d.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.VerifiedByAdmin)
                .WithMany(a => a.VerifiedDoctorDocuments)
                .HasForeignKey(d => d.VerifiedByAdminId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(d => d.DoctorId);

            builder.HasIndex(d => new
            {
                d.DoctorId,
                d.DocumentType
            });
        }
    }
}
