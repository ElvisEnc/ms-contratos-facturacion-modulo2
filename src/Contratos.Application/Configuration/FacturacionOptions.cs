namespace Contratos.Application.Configuration;

// RN-25: la tasa de impuesto es configurable, nunca una constante en Domain.
public sealed class FacturacionOptions
{
    public const string SeccionConfiguracion = "Facturacion";

    public decimal PorcentajeImpuesto { get; set; } = 13m;
}
