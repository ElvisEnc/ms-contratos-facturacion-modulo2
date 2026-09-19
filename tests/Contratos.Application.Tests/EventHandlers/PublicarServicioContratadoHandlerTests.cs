using Contratos.Application.EventHandlers;
using Contratos.Application.IntegrationEvents.Salientes;
using Contratos.Application.ReadModels;
using Contratos.Application.Tests.Fakes;
using Contratos.Domain.Contratos;
using Contratos.Domain.Shared;
using Joseco.DDD.Core.Results;

namespace Contratos.Application.Tests.EventHandlers;

public class PublicarServicioContratadoHandlerTests
{
    [Fact]
    public async Task Handle_TomaNombreYNroIdentificacionDeLaReplica()
    {
        var pacientes = new FakePacienteConocidoStore();
        var publisher = new FakeIntegrationEventPublisher();
        var handler = new PublicarServicioContratadoHandler(pacientes, publisher);
        var pacienteId = PacienteId.From(Guid.NewGuid());
        pacientes.Agregar(new PacienteConocido
        {
            PacienteId = pacienteId,
            Nombre = "Ana Pérez",
            NroIdentificacion = "1234567",
        });
        var evento = new ServicioContratado(ContratoId.New(), pacienteId, TipoServicio.EVALUACION, 1, new DateOnly(2026, 9, 7));

        await handler.Handle(evento, CancellationToken.None);

        var publicado = Assert.IsType<ServicioContratadoIntegrationEvent>(Assert.Single(publisher.Publicados));
        Assert.Equal("Ana Pérez", publicado.PacienteNombre);
        Assert.Equal("1234567", publicado.NroIdentificacion);
    }

    [Fact]
    public async Task Handle_Catering_ConservaLaDuracionEnElEventoDeIntegracion()
    {
        var pacientes = new FakePacienteConocidoStore();
        var publisher = new FakeIntegrationEventPublisher();
        var handler = new PublicarServicioContratadoHandler(pacientes, publisher);
        var pacienteId = PacienteId.From(Guid.NewGuid());
        pacientes.Agregar(new PacienteConocido { PacienteId = pacienteId });
        var evento = new ServicioContratado(ContratoId.New(), pacienteId, TipoServicio.CATERING, 15, new DateOnly(2026, 9, 7));

        await handler.Handle(evento, CancellationToken.None);

        var publicado = Assert.IsType<ServicioContratadoIntegrationEvent>(Assert.Single(publisher.Publicados));
        Assert.Equal(15, publicado.DuracionDias);
    }

    [Fact]
    public async Task Handle_Evaluacion_AnulaLaDuracionAunqueElDominioTraigaUnDia()
    {
        var pacientes = new FakePacienteConocidoStore();
        var publisher = new FakeIntegrationEventPublisher();
        var handler = new PublicarServicioContratadoHandler(pacientes, publisher);
        var pacienteId = PacienteId.From(Guid.NewGuid());
        pacientes.Agregar(new PacienteConocido { PacienteId = pacienteId });
        var evento = new ServicioContratado(ContratoId.New(), pacienteId, TipoServicio.EVALUACION, 1, new DateOnly(2026, 9, 7));

        await handler.Handle(evento, CancellationToken.None);

        var publicado = Assert.IsType<ServicioContratadoIntegrationEvent>(Assert.Single(publisher.Publicados));
        Assert.Null(publicado.DuracionDias);
    }

    [Fact]
    public async Task Handle_Evaluacion_LaFechaDeInicioNuncaEsNula()
    {
        var pacientes = new FakePacienteConocidoStore();
        var publisher = new FakeIntegrationEventPublisher();
        var handler = new PublicarServicioContratadoHandler(pacientes, publisher);
        var pacienteId = PacienteId.From(Guid.NewGuid());
        pacientes.Agregar(new PacienteConocido { PacienteId = pacienteId });
        var fechaContratacion = new DateOnly(2026, 9, 7);
        var evento = new ServicioContratado(ContratoId.New(), pacienteId, TipoServicio.EVALUACION, 1, fechaContratacion);

        await handler.Handle(evento, CancellationToken.None);

        var publicado = Assert.IsType<ServicioContratadoIntegrationEvent>(Assert.Single(publisher.Publicados));
        Assert.Equal(fechaContratacion, publicado.FechaInicio);
    }

    [Fact]
    public async Task Handle_PacienteSinReplica_FallaConPacienteSinReplica()
    {
        var pacientes = new FakePacienteConocidoStore();
        var publisher = new FakeIntegrationEventPublisher();
        var handler = new PublicarServicioContratadoHandler(pacientes, publisher);
        var evento = new ServicioContratado(
            ContratoId.New(), PacienteId.From(Guid.NewGuid()), TipoServicio.EVALUACION, 1, new DateOnly(2026, 9, 7));

        var ex = await Assert.ThrowsAsync<DomainException>(() => handler.Handle(evento, CancellationToken.None));

        Assert.Equal("PACIENTE_SIN_REPLICA", ex.Error.Code);
        Assert.Equal(ErrorType.Failure, ex.Error.Type);
    }
}
