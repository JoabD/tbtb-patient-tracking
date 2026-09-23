using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TbtbPatientTracking.Domain.Entities;

namespace TbtbPatientTracking.Infrastructure.Persistence.Configurations;

public class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        builder.ToTable("Contacts");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.GestorUsername).HasMaxLength(50).IsUnicode(false).IsRequired();

        builder.Property(c => c.ContactDate).IsRequired();

        builder.Property(c => c.Channel).HasConversion<string>().HasMaxLength(20).IsUnicode(false).IsRequired();

        builder.Property(c => c.ResultCode).HasConversion<string>().HasMaxLength(50).IsUnicode(false).IsRequired();

        builder.Property(c => c.Observations).HasMaxLength(500).IsUnicode(false);

        // ValueGeneratedNever evita que EF omita el valor 'false' y la base aplique el default.
        builder.Property(c => c.IsDeleted).HasDefaultValue(false).ValueGeneratedNever();

        builder.Property(c => c.CorrectionReason).HasMaxLength(300).IsUnicode(false);

        builder.Property(c => c.CreatedAt).IsRequired();

        // Un paciente nunca se borra en cascada: los contactos son parte del historial auditable.
        builder.HasOne(c => c.Patient)
            .WithMany(p => p.Contacts)
            .HasForeignKey(c => c.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        // Sirve a "último contacto por paciente" y a la futura regla de los tres últimos contactos (CA-5).
        // Solo indexa los contactos vigentes (no anulados).
        builder.HasIndex(c => new { c.PatientId, c.ContactDate })
            .IsDescending(false, true)
            .HasFilter("[IsDeleted] = 0")
            .HasDatabaseName("IX_Contacts_PatientId_ContactDate");
    }
}
