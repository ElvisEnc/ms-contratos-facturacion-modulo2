using Contratos.Application.Ports;
using Contratos.Domain.Facturas;

namespace Contratos.Application.Tests.Fakes;

public sealed class FakeFacturaRepository : IFacturaRepository
{
    private readonly Dictionary<Guid, Factura> _facturas = [];

    public IReadOnlyCollection<Factura> Facturas => _facturas.Values.ToList();

    public void Agregar(Factura factura) => _facturas[factura.Id] = factura;

    public Task<Factura?> GetByIdAsync(Guid id, bool readOnly = false) =>
        Task.FromResult(_facturas.GetValueOrDefault(id));

    public Task AddAsync(Factura entity)
    {
        _facturas[entity.Id] = entity;
        return Task.CompletedTask;
    }
}
