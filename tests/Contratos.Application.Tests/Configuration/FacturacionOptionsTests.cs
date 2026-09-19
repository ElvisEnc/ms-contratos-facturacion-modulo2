using Contratos.Application.Configuration;

namespace Contratos.Application.Tests.Configuration;

public class FacturacionOptionsTests
{
    // RN-25: el valor por defecto de la tasa de impuesto es 13%. FacturacionOptions
    // no declara una moneda por defecto (ni RN-25 ni docs/DESIGN.md §14.1/§16 la
    // mencionan como propiedad de esta clase), así que este test solo cubre lo
    // que la clase realmente expone.
    [Fact]
    public void Los_valores_por_defecto_son_los_de_RN_25()
    {
        var opciones = new FacturacionOptions();

        Assert.Equal(13m, opciones.PorcentajeImpuesto);
    }
}
