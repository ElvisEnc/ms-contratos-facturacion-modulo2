using Contratos.Domain.Shared;
using MediatR;

namespace Contratos.Application.Queries;

public sealed record GetContratoByIdQuery(Guid ContratoId) : IRequest<ContratoDetalleDto>;

public sealed record ServicioContratadoDto(TipoServicio Tipo, string Nombre, decimal Precio, string Moneda);

public sealed record VigenciaDto(DateOnly FechaInicio, DateOnly FechaFin, int DuracionDias);

public sealed record FacturaResumenDto(Guid FacturaId, EstadoFactura Estado, decimal Total, string Moneda);

public sealed record PagoDto(DateOnly Fecha, decimal Monto, string Moneda, string Referencia);

public sealed record ContratoDetalleDto(
    Guid ContratoId,
    Guid PacienteId,
    ServicioContratadoDto Servicio,
    VigenciaDto Vigencia,
    EstadoContrato Estado,
    FacturaResumenDto Factura,
    PagoDto? Pago);
