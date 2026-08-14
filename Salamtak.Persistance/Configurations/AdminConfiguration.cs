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
    public class AdminConfiguration : IEntityTypeConfiguration<Admin>
    {
        public void Configure(EntityTypeBuilder<Admin> builder)
        {
            builder.ToTable("Admins");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.UserId)
                   .IsRequired();

            builder.HasIndex(a => a.UserId)
                   .IsUnique();

            builder.Property(a => a.Department)
                   .HasMaxLength(100);

            builder.Property(a => a.CreatedAt)
                   .IsRequired();

            builder.Property(a => a.UpdatedAt)
                   .IsRequired(false);

            builder.Property(a => a.IsDeleted)
                   .IsRequired()
                   .HasDefaultValue(false);

            builder.HasOne(a => a.User)
                   .WithOne(u => u.Admin)
                   .HasForeignKey<Admin>(a => a.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(a => a.VerifiedDoctorDocuments)
                   .WithOne(d => d.VerifiedByAdmin)
                   .HasForeignKey(d => d.VerifiedByAdminId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
