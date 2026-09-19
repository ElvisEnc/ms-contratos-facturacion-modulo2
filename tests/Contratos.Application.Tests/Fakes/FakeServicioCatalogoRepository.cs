using Contratos.Application.Ports;
using Contratos.Domain.Catalogo;
using Contratos.Domain.Shared;

namespace Contratos.Application.Tests.Fakes;

public sealed class FakeServicioCatalogoRepository : IServicioCatalogoRepository
{
    private readonly Dictionary<Guid, ServicioCatalogo> _servicios = [];

    public IReadOnlyCollection<ServicioCatalogo> Servicios => _servicios.Values.ToList();

    public void Agregar(ServicioCatalogo servicio) => _servicios[servicio.Id] = servicio;

    public Task<ServicioCatalogo?> GetByIdAsync(Guid id, bool readOnly = false) =>
        Task.FromResult(_servicios.GetValueOrDefault(id));

    public Task AddAsync(ServicioCatalogo entity)
    {
        _servicios[entity.Id] = entity;
        return Task.CompletedTask;
    }

    public Task<ServicioCatalogo?> GetByTipoAsync(TipoServicio tipo) =>
        Task.FromResult(_servicios.Values.FirstOrDefault(s => s.Tipo == tipo));
}
