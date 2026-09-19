using Contratos.Domain.Shared;
using Joseco.DDD.Core.Abstractions;

namespace Contratos.Domain.Contratos;

public sealed record ContratoCancelado(ContratoId ContratoId, FacturaId FacturaId) : DomainEvent;
