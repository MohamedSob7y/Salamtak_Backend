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
    public class PrescriptionConfiguration : IEntityTypeConfiguration<Prescription>
    {
        public void Configure(EntityTypeBuilder<Prescription> builder)
        {
            builder.ToTable("Prescriptions");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.MedicalReportEntryId)
                   .IsRequired();

            builder.Property(p => p.DrugName)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(p => p.Dose)
                   .HasMaxLength(100)
                   .IsRequired(false);

            builder.Property(p => p.Duration)
                   .HasMaxLength(100)
                   .IsRequired(false);

            builder.Property(p => p.Instructions)
                   .HasMaxLength(500)
                   .IsRequired(false);

            builder.Property(p => p.CreatedAt)
                   .IsRequired();

            builder.Property(p => p.UpdatedAt)
                   .IsRequired(false);

            builder.Property(p => p.IsDeleted)
                   .IsRequired()
                   .HasDefaultValue(false);

            builder.HasOne(p => p.MedicalReportEntry)
                   .WithMany(e => e.Prescriptions)
                   .HasForeignKey(p => p.MedicalReportEntryId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
