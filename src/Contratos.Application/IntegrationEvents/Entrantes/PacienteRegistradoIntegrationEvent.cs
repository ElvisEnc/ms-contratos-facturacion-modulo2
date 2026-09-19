namespace Contratos.Application.IntegrationEvents.Entrantes;

// BC1 -> BC3 (SISTEMA.md §9.1). `Contacto` se acepta y se descarta, pero
// tipado — no JsonElement opaco: si el contrato cambia, rompe la
// compilación, no falla en ejecución.
public sealed record PacienteRegistradoIntegrationEvent(
    Guid PacienteId,
    string Nombre,
    string NroIdentificacion,
    Contacto Contacto);

// SISTEMA.md §8: `Contacto.direccion` es texto libre de referencia
// administrativa, no la dirección de entrega (esa es un DireccionGeo, de BC4).
public sealed record Contacto(
    string Telefono,
    string Email,
    string Direccion);
