using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TbtbPatientTracking.Domain.Entities;

namespace TbtbPatientTracking.Infrastructure.Persistence.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("Patients");

        builder.HasKey(p => p.Id);

        // Todo el texto se guarda como varchar (IsUnicode(false)), según el plan.
        builder.Property(p => p.FullName).HasMaxLength(150).IsUnicode(false).IsRequired();

        builder.Property(p => p.DocumentType).HasConversion<string>().HasMaxLength(50).IsUnicode(false).IsRequired();

        builder.Property(p => p.DocumentNumber).HasMaxLength(50).IsUnicode(false).IsRequired();

        // Código ISO de 2 letras (CO, PE, EC), guardado como char(2).
        builder.Property(p => p.Country).HasConversion<string>().HasMaxLength(2).IsFixedLength().IsUnicode(false).IsRequired();

        builder.Property(p => p.City).HasMaxLength(100).IsUnicode(false).IsRequired();

        builder.Property(p => p.Phone).HasMaxLength(20).IsUnicode(false).IsRequired();

        builder.Property(p => p.Email).HasMaxLength(100).IsUnicode(false);

        builder.Property(p => p.TreatmentStartDate).HasColumnType("date");

        builder.Property(p => p.TrackingStatus).HasConversion<string>().HasMaxLength(20).IsUnicode(false).IsRequired();

        // ValueGeneratedNever evita que EF omita el valor 'false' y la base aplique el default 'true'.
        builder.Property(p => p.IsActive).HasDefaultValue(true).ValueGeneratedNever();

        builder.Property(p => p.PrivacyConsentAt).IsRequired();

        builder.Property(p => p.CreatedAt).IsRequired();

        builder.Property(p => p.CreatedBy).HasMaxLength(50).IsUnicode(false).IsRequired();

        // Llave natural: impide pacientes duplicados con el mismo documento en el mismo país.
        builder.HasIndex(p => new { p.Country, p.DocumentType, p.DocumentNumber })
            .IsUnique()
            .HasDatabaseName("UX_Patients_Country_DocumentType_DocumentNumber");
    }
}
