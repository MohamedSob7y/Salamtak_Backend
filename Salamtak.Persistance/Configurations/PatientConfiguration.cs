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
    public class PatientConfiguration : IEntityTypeConfiguration<Patient>
    {
        public void Configure(EntityTypeBuilder<Patient> builder)
        {
            builder.ToTable("Patients");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.UserId)
                   .IsRequired();

            builder.HasIndex(p => p.UserId)
                   .IsUnique();

            builder.Property(p => p.Gender)
                   .IsRequired();

            builder.Property(p => p.DateOfBirth)
                   .IsRequired();

            builder.Property(p => p.Address)
                   .HasMaxLength(250);

            builder.Property(p => p.Height)
                   .IsRequired(false);

            builder.Property(p => p.Weight)
                   .IsRequired(false);

            builder.Property(p => p.CreatedAt)
                   .IsRequired();

            builder.Property(p => p.UpdatedAt)
                   .IsRequired(false);

            builder.Property(p => p.IsDeleted)
                   .IsRequired()
                   .HasDefaultValue(false);

            builder.HasOne(p => p.User)
                   .WithOne(u => u.Patient)
                   .HasForeignKey<Patient>(p => p.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(p => p.MedicalReport)
                   .WithOne(m => m.Patient)
                   .HasForeignKey<MedicalReport>(m => m.PatientId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.Appointments)
                   .WithOne(a => a.Patient)
                   .HasForeignKey(a => a.PatientId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(p => p.Feedbacks)
                   .WithOne(f => f.Patient)
                   .HasForeignKey(f => f.PatientId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
