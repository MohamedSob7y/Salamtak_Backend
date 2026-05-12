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
    public class SpecialtyConfiguration : IEntityTypeConfiguration<Specialty>
    {
        public void Configure(EntityTypeBuilder<Specialty> builder)
        {
            builder.ToTable("Specialties");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.HasIndex(s => s.Name)
                   .IsUnique();

            builder.Property(s => s.Description)
                   .HasMaxLength(500);

            builder.Property(s => s.CreatedAt)
                   .IsRequired();

            builder.Property(s => s.UpdatedAt)
                   .IsRequired(false);

            builder.Property(s => s.IsDeleted)
                   .IsRequired()
                   .HasDefaultValue(false);

            builder.HasMany(s => s.Doctors)
                   .WithOne(d => d.Specialty)
                   .HasForeignKey(d => d.SpecialtyId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
