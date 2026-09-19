using Contratos.Application.Catalogo.DesactivarServicio;
using Contratos.Application.Tests.Fakes;
using Contratos.Domain.Catalogo;
using Contratos.Domain.Shared;
using Joseco.DDD.Core.Results;

namespace Contratos.Application.Tests.Catalogo;

public class DesactivarServicioCommandHandlerTests
{
    [Fact]
    public async Task Handle_HappyPath_DesactivaElServicio()
    {
        var repo = new FakeServicioCatalogoRepository();
        var servicio = ServicioCatalogo.Crear(TipoServicio.EVALUACION, "Evaluación", Money.Crear(100m, "BOB"));
        repo.Agregar(servicio);
        var unitOfWork = new FakeUnitOfWork();
        var handler = new DesactivarServicioCommandHandler(repo, unitOfWork);

        await handler.Handle(new DesactivarServicioCommand(servicio.Id), CancellationToken.None);

        Assert.False(servicio.Activo);
        Assert.Equal(1, unitOfWork.Commits);
    }

    [Fact]
    public async Task Handle_ServicioInexistente_FallaConServicioNoEncontrado()
    {
        var handler = new DesactivarServicioCommandHandler(new FakeServicioCatalogoRepository(), new FakeUnitOfWork());

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            handler.Handle(new DesactivarServicioCommand(Guid.NewGuid()), CancellationToken.None));

        Assert.Equal("SERVICIO_NO_ENCONTRADO", ex.Error.Code);
        Assert.Equal(ErrorType.NotFound, ex.Error.Type);
    }
}
