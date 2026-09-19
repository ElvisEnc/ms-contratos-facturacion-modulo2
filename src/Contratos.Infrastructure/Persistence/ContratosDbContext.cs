using Contratos.Application.ReadModels;
using Contratos.Domain.Catalogo;
using Contratos.Domain.Contratos;
using Contratos.Domain.Controles;
using Contratos.Domain.Facturas;
using Joseco.DDD.Core.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Contratos.Infrastructure.Persistence;

public sealed class ContratosDbContext(DbContextOptions<ContratosDbContext> options) : DbContext(options)
{
    public DbSet<ServicioCatalogo> ServiciosCatalogo => Set<ServicioCatalogo>();
    public DbSet<Contrato> Contratos => Set<Contrato>();
    public DbSet<Factura> Facturas => Set<Factura>();
    public DbSet<ControlAdicional> ControlesAdicionales => Set<ControlAdicional>();
    public DbSet<PacienteConocido> PacientesConocidos => Set<PacienteConocido>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ContratosDbContext).Assembly);

        // Joseco.DDD.Core §2.2: DomainEvents es ICollection<DomainEvent> en Entity;
        // si no se ignora aquí, EF intenta mapear el tipo base y la migración sale
        // con una tabla que nadie pidió.
        modelBuilder.Ignore<DomainEvent>();
    }
}
