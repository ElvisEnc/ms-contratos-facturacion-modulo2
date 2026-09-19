using MediatR;

namespace Contratos.Application.Integracion.ProcesarPacienteRegistrado;

// Adaptador de entrada, no un caso de uso del negocio: refleja exactamente
// los tres campos que Application usa de PacienteRegistradoIntegrationEvent
// (contacto se recibe y se descarta antes de llegar aquí).
public sealed record ProcesarPacienteRegistradoCommand(
    Guid PacienteId,
    string Nombre,
    string NroIdentificacion) : IRequest;
