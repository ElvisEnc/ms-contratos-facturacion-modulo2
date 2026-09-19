using Contratos.Domain.Facturas;
using Contratos.Domain.Shared;
using Joseco.DDD.Core.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contratos.Infrastructure.Persistence.Configurations;

public sealed class FacturaConfig : IEntityTypeConfiguration<Factura>
{
    public void Configure(EntityTypeBuilder<Factura> builder)
    {
        builder.ToTable("facturas");

        builder.Ignore(f => f.Id);
        builder.Property(f => f.FacturaId)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => FacturaId.From(value))
            .ValueGeneratedNever();
        builder.HasKey(f => f.FacturaId);

        builder.Property(f => f.PacienteId)
            .HasColumnName("paciente_id")
            .HasConversion(id => id.Value, value => PacienteId.From(value))
            .IsRequired();

        builder.Property(f => f.Origen)
            .HasColumnName("origen")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // Guid crudo: es un ContratoId o un ControlId según Origen (§5.7).
        // Referencia entre agregados: sin FK.
        builder.Property(f => f.ReferenciaId)
            .HasColumnName("referencia_id")
            .IsRequired();

        builder.Property(f => f.Fecha)
            .HasColumnName("fecha")
            .IsRequired();

        builder.Property(f => f.Estado)
            .HasColumnName("estado")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.OwnsOne(f => f.Subtotal, m =>
        {
            m.Property(x => x.Monto).HasColumnName("subtotal_monto").HasPrecision(18, 2);
            m.Property(x => x.Moneda).HasColumnName("subtotal_moneda").HasMaxLength(3);
        });
        builder.Navigation(f => f.Subtotal).IsRequired();

        builder.OwnsOne(f => f.Impuesto, m =>
        {
            m.Property(x => x.Monto).HasColumnName("impuesto_monto").HasPrecision(18, 2);
            m.Property(x => x.Moneda).HasColumnName("impuesto_moneda").HasMaxLength(3);
        });
        builder.Navigation(f => f.Impuesto).IsRequired();

        builder.OwnsOne(f => f.Total, m =>
        {
            m.Property(x => x.Monto).HasColumnName("total_monto").HasPrecision(18, 2);
            m.Property(x => x.Moneda).HasColumnName("total_moneda").HasMaxLength(3);
        });
        builder.Navigation(f => f.Total).IsRequired();

        // Factura.Lineas es la única colección del microservicio (§12.2 del maestro).
        // Entidad hija referenciable con LineaFacturaId, nunca OwnsMany con clave
        // sombra int autogenerada (D-08). El Ignore evita que EF descubra "Lineas"
        // por convención y choque con la navegación explícita por "_lineas".
        // IsRequired(): una línea siempre pertenece a una factura; sin esto EF
        // deja factura_id nullable por defecto en un WithOne() sin principal
        // explícito.
        builder.HasMany(typeof(LineaFactura), "_lineas")
            .WithOne()
            .HasForeignKey("factura_id")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        builder.Ignore(f => f.Lineas);

        builder.HasIndex(f => f.PacienteId).HasDatabaseName("ix_facturas_paciente_id");
        builder.HasIndex(f => f.ReferenciaId).HasDatabaseName("ix_facturas_referencia_id");

        builder.Ignore(f => f.DomainEvents);
    }
}
