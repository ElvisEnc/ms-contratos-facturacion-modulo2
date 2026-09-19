using Contratos.Domain.Contratos;
using Contratos.Domain.Shared;
using Joseco.DDD.Core.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contratos.Infrastructure.Persistence.Configurations;

public sealed class ContratoConfig : IEntityTypeConfiguration<Contrato>
{
    public void Configure(EntityTypeBuilder<Contrato> builder)
    {
        builder.ToTable("contratos");

        builder.Ignore(c => c.Id);
        builder.Property(c => c.ContratoId)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => ContratoId.From(value))
            .ValueGeneratedNever();
        builder.HasKey(c => c.ContratoId);

        builder.Property(c => c.PacienteId)
            .HasColumnName("paciente_id")
            .HasConversion(id => id.Value, value => PacienteId.From(value))
            .IsRequired();

        builder.Property(c => c.FacturaId)
            .HasColumnName("factura_id")
            .HasConversion(id => id.Value, value => FacturaId.From(value))
            .IsRequired();

        builder.Property(c => c.FechaContratacion)
            .HasColumnName("fecha_contratacion")
            .IsRequired();

        builder.Property(c => c.Estado)
            .HasColumnName("estado")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // servicio_tipo en minúsculas explícito: el índice único filtrado lo referencia literal.
        builder.OwnsOne(c => c.Servicio, servicio =>
        {
            servicio.Property(s => s.Tipo)
                .HasColumnName("servicio_tipo")
                .HasConversion<string>()
                .HasMaxLength(20);
            servicio.Property(s => s.Nombre).HasColumnName("servicio_nombre");
            servicio.OwnsOne(s => s.Precio, precio =>
            {
                precio.Property(m => m.Monto).HasColumnName("servicio_precio_monto").HasPrecision(18, 2);
                precio.Property(m => m.Moneda).HasColumnName("servicio_precio_moneda").HasMaxLength(3);
            });
        });
        builder.Navigation(c => c.Servicio).IsRequired();

        // Los tres campos se persisten aunque FechaFin sea derivable: reconstruir
        // el VO exige que su constructor valide la coherencia de los tres (§14.2).
        builder.OwnsOne(c => c.Vigencia, vigencia =>
        {
            vigencia.Property(v => v.FechaInicio).HasColumnName("vigencia_fecha_inicio");
            vigencia.Property(v => v.FechaFin).HasColumnName("vigencia_fecha_fin");
            vigencia.Property(v => v.DuracionDias).HasColumnName("vigencia_duracion_dias");
        });
        builder.Navigation(c => c.Vigencia).IsRequired();

        // Pago es 0..1: sin IsRequired(); las cuatro columnas en null significan "sin pago".
        builder.OwnsOne(c => c.Pago, pago =>
        {
            pago.Property(p => p.Fecha).HasColumnName("pago_fecha");
            pago.Property(p => p.Referencia).HasColumnName("pago_referencia").HasMaxLength(200);
            pago.OwnsOne(p => p.Monto, precio =>
            {
                precio.Property(m => m.Monto).HasColumnName("pago_monto").HasPrecision(18, 2);
                precio.Property(m => m.Moneda).HasColumnName("pago_moneda").HasMaxLength(3);
            });
        });

        // C3/RN-19 (D-05): respaldo físico único ahora que C12 (cierre perezoso)
        // garantiza que ningún contrato vencido conserva ACTIVO al recontratar.
        // El nombre va en HasIndex(..., "nombre"), no solo en HasDatabaseName():
        // dos HasIndex() sobre la MISMA propiedad sin nombre en la firma se
        // fusionan en un único índice (EF Core reutiliza el IndexBuilder por
        // lista de propiedades), y el segundo Ignore/HasDatabaseName solo
        // renombra el primero en vez de crear uno nuevo.
        builder.HasIndex(c => c.PacienteId, "ux_contratos_catering_activo_por_paciente")
            .IsUnique()
            .HasFilter("servicio_tipo = 'CATERING' AND estado = 'ACTIVO'");

        builder.HasIndex(c => c.PacienteId, "ix_contratos_paciente_id");
        builder.HasIndex(c => c.FechaContratacion, "ix_contratos_fecha_contratacion");

        builder.Ignore(c => c.DomainEvents);
    }
}
