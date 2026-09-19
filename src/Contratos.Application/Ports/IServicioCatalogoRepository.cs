using Contratos.Domain.Catalogo;
using Contratos.Domain.Shared;
using Joseco.DDD.Core.Abstractions;

namespace Contratos.Application.Ports;

public interface IServicioCatalogoRepository : IRepository<ServicioCatalogo>
{
    // C9, y resolución del cobro de un control adicional (C7).
    public Task<ServicioCatalogo?> GetByTipoAsync(TipoServicio tipo);
}
