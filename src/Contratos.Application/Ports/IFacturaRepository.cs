using Contratos.Domain.Facturas;
using Joseco.DDD.Core.Abstractions;

namespace Contratos.Application.Ports;

// GetByIdAsync se implementa con Include(f => f.Lineas): una factura sin sus
// líneas no es una factura, y C11 no se puede verificar sobre ella.
public interface IFacturaRepository : IRepository<Factura>;
