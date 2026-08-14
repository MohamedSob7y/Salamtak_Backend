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
    public class MedicalReportAttachmentConfiguration: IEntityTypeConfiguration<MedicalReportAttachment>
    {
        public void Configure(EntityTypeBuilder<MedicalReportAttachment> builder)
        {
            builder.ToTable("MedicalReportAttachments");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.OriginalFileName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(a => a.StoredFileName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(a => a.StoragePath)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(a => a.ContentType)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(a => a.FileSize)
                .IsRequired();

            builder.Property(a => a.Description)
                .IsRequired(false)
                .HasMaxLength(500);

            builder.Property(a => a.UploadedByType)
                .IsRequired();

            
            builder.HasOne(a => a.MedicalReport)
                .WithMany(r => r.Attachments)
                .HasForeignKey(a => a.MedicalReportId)
                .OnDelete(DeleteBehavior.Restrict);

           
            builder.HasOne(a => a.MedicalReportEntry)
                .WithMany(e => e.Attachments)
                .HasForeignKey(a => a.MedicalReportEntryId)
                .OnDelete(DeleteBehavior.Restrict);

           
            builder.HasOne(a => a.UploadedByUser)
                .WithMany()
                .HasForeignKey(a => a.UploadedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            
            builder.HasIndex(a => a.MedicalReportId);

            builder.HasIndex(a => a.MedicalReportEntryId);

            builder.HasIndex(a => a.UploadedByUserId);
        }
    }
}
