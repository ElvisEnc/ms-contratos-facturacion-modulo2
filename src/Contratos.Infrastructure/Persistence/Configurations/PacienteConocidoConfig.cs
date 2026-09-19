using Contratos.Application.ReadModels;
using Contratos.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contratos.Infrastructure.Persistence.Configurations;

// Read model, no agregado: sin DomainEvents, sin IRepository<T> (§12.9 del maestro).
public sealed class PacienteConocidoConfig : IEntityTypeConfiguration<PacienteConocido>
{
    public void Configure(EntityTypeBuilder<PacienteConocido> builder)
    {
        builder.ToTable("pacientes_conocidos");

        builder.Property(p => p.PacienteId)
            .HasColumnName("paciente_id")
            .HasConversion(id => id.Value, value => PacienteId.From(value))
            .ValueGeneratedNever();
        builder.HasKey(p => p.PacienteId);

        builder.Property(p => p.Nombre).HasColumnName("nombre").IsRequired();
        builder.Property(p => p.NroIdentificacion).HasColumnName("nro_identificacion").IsRequired();
        builder.Property(p => p.PlanId).HasColumnName("plan_id");
        builder.Property(p => p.TienePlanActivo).HasColumnName("tiene_plan_activo").IsRequired();
        builder.Property(p => p.PlanFechaInicio).HasColumnName("plan_fecha_inicio");
        builder.Property(p => p.PlanDuracionDias).HasColumnName("plan_duracion_dias");
    }
}
