using Contratos.Domain.Controles;
using Contratos.Domain.Shared;
using Joseco.DDD.Core.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contratos.Infrastructure.Persistence.Configurations;

public sealed class ControlAdicionalConfig : IEntityTypeConfiguration<ControlAdicional>
{
    public void Configure(EntityTypeBuilder<ControlAdicional> builder)
    {
        builder.ToTable("controles_adicionales");

        builder.Ignore(c => c.Id);
        builder.Property(c => c.ControlId)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => ControlId.From(value))
            .ValueGeneratedNever();
        builder.HasKey(c => c.ControlId);

        builder.Property(c => c.PacienteId)
            .HasColumnName("paciente_id")
            .HasConversion(id => id.Value, value => PacienteId.From(value))
            .IsRequired();

        builder.Property(c => c.EvaluacionId)
            .HasColumnName("evaluacion_id")
            .HasConversion(id => id.Value, value => EvaluacionId.From(value))
            .IsRequired();

        builder.Property(c => c.FacturaId)
            .HasColumnName("factura_id")
            .HasConversion(id => id.Value, value => FacturaId.From(value))
            .IsRequired();

        builder.Property(c => c.Fecha)
            .HasColumnName("fecha")
            .IsRequired();

        builder.OwnsOne(c => c.Cobro, m =>
        {
            m.Property(x => x.Monto).HasColumnName("cobro_monto").HasPrecision(18, 2);
            m.Property(x => x.Moneda).HasColumnName("cobro_moneda").HasMaxLength(3);
        });
        builder.Navigation(c => c.Cobro).IsRequired();

        builder.Property(c => c.Pagado)
            .HasColumnName("pagado")
            .IsRequired();

        // C10: respaldo físico de "un control por evaluación", pase lo que pase
        // con las entregas duplicadas de un bus at-least-once.
        builder.HasIndex(c => c.EvaluacionId)
            .IsUnique()
            .HasDatabaseName("ux_controles_evaluacion_id");

        builder.HasIndex(c => c.PacienteId).HasDatabaseName("ix_controles_adicionales_paciente_id");

        builder.Ignore(c => c.DomainEvents);
    }
}
