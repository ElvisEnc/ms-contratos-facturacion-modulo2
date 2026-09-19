using Contratos.Domain.Shared;

namespace Contratos.Domain.Facturas;

// Insumo público para Factura.Emitir: la línea real (LineaFactura) solo la
// construye el agregado, con su LineaFacturaId y su Money clonado.
public sealed record DetalleLinea(string Concepto, int Cantidad, Money PrecioUnitario);
