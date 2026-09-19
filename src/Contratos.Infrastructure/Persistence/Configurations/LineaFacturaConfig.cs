using Contratos.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contratos.Infrastructure.Persistence.Configurations;

public sealed class LineaFacturaConfig : IEntityTypeConfiguration<LineaFactura>
{
    public void Configure(EntityTypeBuilder<LineaFactura> builder)
    {
        builder.ToTable("factura_lineas");

        builder.Ignore(l => l.Id);
        builder.Property(l => l.LineaId)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => LineaFacturaId.From(value))
            .ValueGeneratedNever();
        builder.HasKey(l => l.LineaId);

        builder.Property(l => l.Concepto)
            .HasColumnName("concepto")
            .IsRequired();

        builder.Property(l => l.Cantidad)
            .HasColumnName("cantidad")
            .IsRequired();

        builder.OwnsOne(l => l.PrecioUnitario, m =>
        {
            m.Property(x => x.Monto).HasColumnName("precio_unitario_monto").HasPrecision(18, 2);
            m.Property(x => x.Moneda).HasColumnName("precio_unitario_moneda").HasMaxLength(3);
        });
        builder.Navigation(l => l.PrecioUnitario).IsRequired();

        // C19: el subtotal se calcula (Cantidad × PrecioUnitario), nunca se persiste.
        builder.Ignore(l => l.Subtotal);
    }
}
