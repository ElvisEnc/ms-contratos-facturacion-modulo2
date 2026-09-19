using Contratos.Application.Catalogo.ActualizarPrecio;
using Contratos.Application.Tests.Fakes;
using Contratos.Domain.Catalogo;
using Contratos.Domain.Shared;
using Joseco.DDD.Core.Results;

namespace Contratos.Application.Tests.Catalogo;

public class ActualizarPrecioCommandHandlerTests
{
    [Fact]
    public async Task Handle_HappyPath_ActualizaElPrecioVigente()
    {
        var repo = new FakeServicioCatalogoRepository();
        var servicio = ServicioCatalogo.Crear(TipoServicio.EVALUACION, "Evaluación", Money.Crear(100m, "BOB"));
        repo.Agregar(servicio);
        var unitOfWork = new FakeUnitOfWork();
        var handler = new ActualizarPrecioCommandHandler(repo, unitOfWork);

        await handler.Handle(new ActualizarPrecioCommand(servicio.Id, 150m, "BOB"), CancellationToken.None);

        Assert.Equal(150m, servicio.PrecioVigente.Monto);
        Assert.Equal(1, unitOfWork.Commits);
    }

    [Fact]
    public async Task Handle_ServicioInexistente_FallaConServicioNoEncontrado()
    {
        var handler = new ActualizarPrecioCommandHandler(new FakeServicioCatalogoRepository(), new FakeUnitOfWork());

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            handler.Handle(new ActualizarPrecioCommand(Guid.NewGuid(), 150m, "BOB"), CancellationToken.None));

        Assert.Equal("SERVICIO_NO_ENCONTRADO", ex.Error.Code);
        Assert.Equal(ErrorType.NotFound, ex.Error.Type);
    }
}
