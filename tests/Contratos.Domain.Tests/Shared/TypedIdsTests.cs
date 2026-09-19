using Contratos.Domain.Shared;
using Joseco.DDD.Core.Results;

namespace Contratos.Domain.Tests.Shared;

public class TypedIdsTests
{
    private static void AsertarFallaConIdVacio(Action accion)
    {
        var ex = Assert.Throws<DomainException>(accion);

        Assert.Equal("ID_VACIO", ex.Error.Code);
        Assert.Equal(ErrorType.Validation, ex.Error.Type);
    }

    [Fact]
    public void ServicioCatalogoId_From_ConGuidVacio_Falla() =>
        AsertarFallaConIdVacio(() => ServicioCatalogoId.From(Guid.Empty));

    [Fact]
    public void ContratoId_From_ConGuidVacio_Falla() =>
        AsertarFallaConIdVacio(() => ContratoId.From(Guid.Empty));

    [Fact]
    public void FacturaId_From_ConGuidVacio_Falla() =>
        AsertarFallaConIdVacio(() => FacturaId.From(Guid.Empty));

    [Fact]
    public void LineaFacturaId_From_ConGuidVacio_Falla() =>
        AsertarFallaConIdVacio(() => LineaFacturaId.From(Guid.Empty));

    [Fact]
    public void ControlId_From_ConGuidVacio_Falla() =>
        AsertarFallaConIdVacio(() => ControlId.From(Guid.Empty));

    [Fact]
    public void PacienteId_From_ConGuidVacio_Falla() =>
        AsertarFallaConIdVacio(() => PacienteId.From(Guid.Empty));

    [Fact]
    public void EvaluacionId_From_ConGuidVacio_Falla() =>
        AsertarFallaConIdVacio(() => EvaluacionId.From(Guid.Empty));

    [Fact]
    public void New_ProduceUnIdNoVacio()
    {
        Assert.NotEqual(Guid.Empty, ServicioCatalogoId.New().Value);
        Assert.NotEqual(Guid.Empty, ContratoId.New().Value);
        Assert.NotEqual(Guid.Empty, FacturaId.New().Value);
        Assert.NotEqual(Guid.Empty, LineaFacturaId.New().Value);
        Assert.NotEqual(Guid.Empty, ControlId.New().Value);
    }
}
