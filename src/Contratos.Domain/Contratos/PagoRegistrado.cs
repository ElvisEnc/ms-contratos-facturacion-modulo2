using Contratos.Domain.Shared;
using Joseco.DDD.Core.Abstractions;

namespace Contratos.Domain.Contratos;

public sealed record PagoRegistrado(ContratoId ContratoId, Pago Pago) : DomainEvent;
