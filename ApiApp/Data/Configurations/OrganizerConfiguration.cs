using ApiApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiApp.Data.Configurations
{
    public class OrganizerConfiguration : IEntityTypeConfiguration<Organizer>
    {
        public void Configure(EntityTypeBuilder<Organizer> builder)
        {
            builder.HasKey(o => o.Id);

            builder.Property(o => o.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(o => o.Email)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(o => o.Email)
                .IsUnique();

            builder.Property(o => o.Phone)
                .HasMaxLength(20);

            builder.Property(o => o.LogoUrl)
                .HasMaxLength(500);
        }
    }
}
