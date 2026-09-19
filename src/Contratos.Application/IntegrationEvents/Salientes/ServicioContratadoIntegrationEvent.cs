using Contratos.Domain.Shared;

namespace Contratos.Application.IntegrationEvents.Salientes;

// BC3 -> BC1, BC2, BC4 (SISTEMA.md §9.2). Compuesto por la política
// PublicarServicioContratadoHandler, nunca directamente por el agregado.
public sealed record ServicioContratadoIntegrationEvent(
    Guid ContratoId,
    Guid PacienteId,
    string PacienteNombre,
    string NroIdentificacion,
    TipoServicio TipoServicio,
    int? DuracionDias,   // nulo salvo CATERING (C16)
    DateOnly FechaInicio); // nunca nulo, ni siquiera fuera de CATERING
