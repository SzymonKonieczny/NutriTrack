using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NutriTrack.Domain.Entities;
using NutriTrack.Domain.Enums;

namespace NutriTrack.Domain.Data.Configurations;

public class MicronutrientConfiguration : IEntityTypeConfiguration<Micronutrient>
{
    public void Configure(EntityTypeBuilder<Micronutrient> builder)
    {
        builder.ToTable("Micronutrients");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(e => e.DailyReferenceAmount)
               .HasColumnType("decimal(18,4)");

        builder.Property(e => e.Unit)
               .HasConversion<string>()
               .HasMaxLength(50);
    }
}