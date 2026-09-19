using Contratos.Domain.Catalogo;
using Contratos.Domain.Shared;
using Joseco.DDD.Core.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contratos.Infrastructure.Persistence.Configurations;

public sealed class ServicioCatalogoConfig : IEntityTypeConfiguration<ServicioCatalogo>
{
    public void Configure(EntityTypeBuilder<ServicioCatalogo> builder)
    {
        builder.ToTable("servicios_catalogo");

        // ServicioCatalogoId se deriva del Id de la raíz, no se persiste aparte (§14.2).
        builder.Ignore(s => s.Id);
        builder.Property(s => s.ServicioId)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => ServicioCatalogoId.From(value))
            .ValueGeneratedNever();
        builder.HasKey(s => s.ServicioId);

        builder.Property(s => s.Tipo)
            .HasColumnName("tipo")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(s => s.Nombre)
            .HasColumnName("nombre")
            .IsRequired();

        builder.OwnsOne(s => s.PrecioVigente, precio =>
        {
            precio.Property(m => m.Monto).HasColumnName("precio_monto").HasPrecision(18, 2);
            precio.Property(m => m.Moneda).HasColumnName("precio_moneda").HasMaxLength(3);
        });
        builder.Navigation(s => s.PrecioVigente).IsRequired();

        builder.Property(s => s.Activo)
            .HasColumnName("activo")
            .IsRequired();

        // C15/D-13: precio determinista por tipo — sin esto, dos servicios activos
        // del mismo tipo dejan el precio contratado a merced del orden de inserción.
        builder.HasIndex(s => s.Tipo)
            .IsUnique()
            .HasFilter("activo = true")
            .HasDatabaseName("ux_servicios_catalogo_tipo_activo");

        builder.Ignore(s => s.DomainEvents);
    }
}
