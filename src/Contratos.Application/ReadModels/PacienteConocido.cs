using Contratos.Domain.Shared;

namespace Contratos.Application.ReadModels;

// No es un agregado: sin invariantes de dominio, sin IRepository<T>, sin
// eventos. Su corrección depende enteramente de la idempotencia de los dos
// eventos que lo alimentan (§9.4, §12.9 del maestro).
public sealed class PacienteConocido
{
    public required PacienteId PacienteId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string NroIdentificacion { get; set; } = string.Empty;

    // D-12 (§9.15 del maestro): distingue "reprocesan el mismo plan" de "llegó un plan distinto".
    public Guid? PlanId { get; set; }
    public bool TienePlanActivo { get; set; }

    // C4 (RN-01b): la vigencia del catering se deriva de aquí, nunca del command.
    public DateOnly? PlanFechaInicio { get; set; }
    public int? PlanDuracionDias { get; set; }
}
