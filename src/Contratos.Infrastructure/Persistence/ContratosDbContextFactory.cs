using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Contratos.Infrastructure.Persistence;

// Solo para herramientas de diseño (dotnet ef migrations add/database update),
// antes de que Contratos.WebApi componga su propio contenedor de DI en fase 5
// (§14 del DESIGN: cadena de conexión de desarrollo local).
public sealed class ContratosDbContextFactory : IDesignTimeDbContextFactory<ContratosDbContext>
{
    public ContratosDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ContratosDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=contratos_db;Username=postgres;Password=postgres");

        return new ContratosDbContext(optionsBuilder.Options);
    }
}
