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
    public class MedicalReportConfiguration : IEntityTypeConfiguration<MedicalReport>
    {
        public void Configure(EntityTypeBuilder<MedicalReport> builder)
        {
            builder.ToTable("MedicalReports");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.PatientId)
                   .IsRequired();

            builder.HasIndex(m => m.PatientId)
                   .IsUnique();

            builder.Property(m => m.CreatedAt)
                   .IsRequired();

            builder.Property(m => m.UpdatedAt)
                   .IsRequired(false);

            builder.Property(m => m.IsDeleted)
                   .IsRequired()
                   .HasDefaultValue(false);

            builder.HasOne(m => m.Patient)
                   .WithOne(p => p.MedicalReport)
                   .HasForeignKey<MedicalReport>(m => m.PatientId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(m => m.Entries)
                   .WithOne(e => e.MedicalReport)
                   .HasForeignKey(e => e.MedicalReportId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
