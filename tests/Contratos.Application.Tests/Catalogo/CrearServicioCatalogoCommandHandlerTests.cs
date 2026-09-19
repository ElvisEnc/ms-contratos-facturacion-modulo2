using Contratos.Application.Catalogo.CrearServicioCatalogo;
using Contratos.Application.Tests.Fakes;
using Contratos.Domain.Shared;

namespace Contratos.Application.Tests.Catalogo;

public class CrearServicioCatalogoCommandHandlerTests
{
    [Fact]
    public async Task Handle_HappyPath_CreaYPersisteElServicio()
    {
        var repo = new FakeServicioCatalogoRepository();
        var unitOfWork = new FakeUnitOfWork();
        var handler = new CrearServicioCatalogoCommandHandler(repo, unitOfWork);
        var command = new CrearServicioCatalogoCommand(TipoServicio.EVALUACION, "Evaluación corporal y consulta", 150m, "BOB");

        var servicioId = await handler.Handle(command, CancellationToken.None);

        var servicio = await repo.GetByIdAsync(servicioId);
        Assert.NotNull(servicio);
        Assert.Equal("Evaluación corporal y consulta", servicio!.Nombre);
        Assert.Equal(150m, servicio.PrecioVigente.Monto);
        Assert.True(servicio.Activo);
        Assert.Equal(1, unitOfWork.Commits);
    }
}
