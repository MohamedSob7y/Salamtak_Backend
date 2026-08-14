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
    public class DoctorClinicConfiguration: IEntityTypeConfiguration<DoctorClinic>
    {
        public void Configure(
            EntityTypeBuilder<DoctorClinic> builder)
        {
            builder.ToTable("DoctorClinics");

            builder.HasKey(dc => dc.Id);

            builder.Property(dc => dc.DoctorId)
                   .IsRequired();

            builder.Property(dc => dc.ClinicId)
                   .IsRequired();

            builder.Property(dc => dc.CreatedAt)
                   .IsRequired();

            builder.Property(dc => dc.UpdatedAt)
                   .IsRequired(false);

            builder.Property(dc => dc.IsDeleted)
                   .IsRequired()
                   .HasDefaultValue(false);

            builder.HasOne(dc => dc.Doctor)
                   .WithMany(d => d.DoctorClinics)
                   .HasForeignKey(dc => dc.DoctorId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(dc => dc.Clinic)
                   .WithMany(c => c.DoctorClinics)
                   .HasForeignKey(dc => dc.ClinicId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(dc => new
            {
                dc.DoctorId,
                dc.ClinicId
            })
            .IsUnique();
        }
    }
}
