using Contratos.Application.Configuration;
using Contratos.Application.Contratos.ContratarServicio;
using Contratos.Application.ReadModels;
using Contratos.Application.Tests.Fakes;
using Contratos.Domain.Catalogo;
using Contratos.Domain.Contratos;
using Contratos.Domain.Shared;
using Joseco.DDD.Core.Results;

namespace Contratos.Application.Tests.Contratos;

public class ContratarServicioCommandHandlerTests
{
    private sealed record Escenario(
        ContratarServicioCommandHandler Handler,
        FakePacienteConocidoStore Pacientes,
        FakeServicioCatalogoRepository Catalogo,
        FakeContratoRepository Contratos,
        FakeFacturaRepository Facturas,
        FakeUnitOfWork UnitOfWork);

    private static Escenario CrearEscenario(decimal porcentajeImpuesto = 13m)
    {
        var pacientes = new FakePacienteConocidoStore();
        var catalogo = new FakeServicioCatalogoRepository();
        var contratos = new FakeContratoRepository();
        var facturas = new FakeFacturaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var opciones = new FacturacionOptions { PorcentajeImpuesto = porcentajeImpuesto };

        var handler = new ContratarServicioCommandHandler(
            pacientes, catalogo, contratos, facturas, opciones, unitOfWork);

        return new Escenario(handler, pacientes, catalogo, contratos, facturas, unitOfWork);
    }

    private static ServicioCatalogo CrearServicioActivo(
        TipoServicio tipo, decimal precio = 100m, string moneda = "BOB", string nombre = "Servicio de prueba")
    {
        var servicio = ServicioCatalogo.Crear(tipo, nombre, Money.Crear(precio, moneda));
        return servicio;
    }

    private static PacienteConocido CrearPaciente(
        Guid pacienteId,
        bool tienePlanActivo = false,
        DateOnly? planFechaInicio = null,
        int? planDuracionDias = null) => new()
        {
            PacienteId = PacienteId.From(pacienteId),
            Nombre = "Paciente de prueba",
            NroIdentificacion = "1234567",
            TienePlanActivo = tienePlanActivo,
            PlanFechaInicio = planFechaInicio,
            PlanDuracionDias = planDuracionDias,
        };

    [Fact]
    public async Task Handle_PacienteDesconocido_FallaConC1()
    {
        var e = CrearEscenario();
        var command = new ContratarServicioCommand(Guid.NewGuid(), TipoServicio.EVALUACION, null);

        var ex = await Assert.ThrowsAsync<DomainException>(() => e.Handler.Handle(command, CancellationToken.None));

        Assert.Equal("C1_PACIENTE_DESCONOCIDO", ex.Error.Code);
        Assert.Equal(ErrorType.Validation, ex.Error.Type);
    }

    [Fact]
    public async Task Handle_ServicioAusenteDelCatalogo_FallaConCatalogoMalConfiguradoYEsFailure()
    {
        var e = CrearEscenario();
        var pacienteId = Guid.NewGuid();
        e.Pacientes.Agregar(CrearPaciente(pacienteId));
        var command = new ContratarServicioCommand(pacienteId, TipoServicio.EVALUACION, null);

        var ex = await Assert.ThrowsAsync<DomainException>(() => e.Handler.Handle(command, CancellationToken.None));

        Assert.Equal("CATALOGO_MAL_CONFIGURADO", ex.Error.Code);
        Assert.Equal(ErrorType.Failure, ex.Error.Type);
    }

    [Fact]
    public async Task Handle_ServicioInactivo_FallaConC9()
    {
        var e = CrearEscenario();
        var pacienteId = Guid.NewGuid();
        e.Pacientes.Agregar(CrearPaciente(pacienteId));
        var servicio = CrearServicioActivo(TipoServicio.EVALUACION);
        servicio.Desactivar();
        e.Catalogo.Agregar(servicio);
        var command = new ContratarServicioCommand(pacienteId, TipoServicio.EVALUACION, null);

        var ex = await Assert.ThrowsAsync<DomainException>(() => e.Handler.Handle(command, CancellationToken.None));

        Assert.Equal("C9_SERVICIO_INACTIVO", ex.Error.Code);
        Assert.Equal(ErrorType.Validation, ex.Error.Type);
    }

    [Fact]
    public async Task Handle_CateringSinPlanActivo_FallaConC2()
    {
        var e = CrearEscenario();
        var pacienteId = Guid.NewGuid();
        e.Pacientes.Agregar(CrearPaciente(pacienteId, tienePlanActivo: false));
        e.Catalogo.Agregar(CrearServicioActivo(TipoServicio.CATERING));
        var command = new ContratarServicioCommand(pacienteId, TipoServicio.CATERING, null);

        var ex = await Assert.ThrowsAsync<DomainException>(() => e.Handler.Handle(command, CancellationToken.None));

        Assert.Equal("C2_SIN_PLAN_ACTIVO", ex.Error.Code);
        Assert.Equal(ErrorType.Validation, ex.Error.Type);
    }

    [Fact]
    public async Task Handle_CateringConDuracionDePlanFueraDe15O30_FallaConC4()
    {
        var e = CrearEscenario();
        var pacienteId = Guid.NewGuid();
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        e.Pacientes.Agregar(CrearPaciente(pacienteId, tienePlanActivo: true, planFechaInicio: hoy, planDuracionDias: 20));
        e.Catalogo.Agregar(CrearServicioActivo(TipoServicio.CATERING));
        var command = new ContratarServicioCommand(pacienteId, TipoServicio.CATERING, null);

        var ex = await Assert.ThrowsAsync<DomainException>(() => e.Handler.Handle(command, CancellationToken.None));

        Assert.Equal("C4_DURACION_INVALIDA", ex.Error.Code);
        Assert.Equal(ErrorType.Validation, ex.Error.Type);
    }

    [Fact]
    public async Task Handle_SegundoCateringActivo_FallaConC3()
    {
        var e = CrearEscenario();
        var pacienteId = Guid.NewGuid();
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);

        // Catering vigente que cubre "hoy" y por tanto también la fecha de inicio
        // del nuevo plan replicado: no está vencido, así que C12 no lo cierra antes.
        var servicioExistente = ServicioCatalogo.Crear(TipoServicio.CATERING, "Catering", Money.Crear(500m, "BOB"));
        var contratoExistente = Contrato.Contratar(
            ContratoId.New(),
            PacienteId.From(pacienteId),
            servicioExistente.ObtenerServicioParaContrato(),
            Vigencia.Desde(hoy, 30),
            FacturaId.New());
        e.Contratos.Agregar(contratoExistente);

        e.Pacientes.Agregar(CrearPaciente(pacienteId, tienePlanActivo: true, planFechaInicio: hoy, planDuracionDias: 15));
        e.Catalogo.Agregar(CrearServicioActivo(TipoServicio.CATERING));
        var command = new ContratarServicioCommand(pacienteId, TipoServicio.CATERING, null);

        var ex = await Assert.ThrowsAsync<DomainException>(() => e.Handler.Handle(command, CancellationToken.None));

        Assert.Equal("C3_CATERING_YA_ACTIVO", ex.Error.Code);
        Assert.Equal(ErrorType.Conflict, ex.Error.Type);
    }

    [Fact]
    public async Task Handle_SegundoCateringConVigenciaQueSolapaEnFechaDistinta_FallaConC3()
    {
        var e = CrearEscenario();
        var pacienteId = Guid.NewGuid();
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var inicioAnterior = hoy.AddDays(-10); // no coincide con la fecha de inicio del nuevo plan

        // Catering anterior: [hoy-10, hoy+19]. Nuevo plan: empieza "hoy", que cae
        // dentro de esa vigencia aunque las dos fechas de inicio sean distintas.
        var servicioExistente = ServicioCatalogo.Crear(TipoServicio.CATERING, "Catering", Money.Crear(500m, "BOB"));
        var contratoExistente = Contrato.Contratar(
            ContratoId.New(),
            PacienteId.From(pacienteId),
            servicioExistente.ObtenerServicioParaContrato(),
            Vigencia.Desde(inicioAnterior, 30),
            FacturaId.New());
        e.Contratos.Agregar(contratoExistente);

        e.Pacientes.Agregar(CrearPaciente(pacienteId, tienePlanActivo: true, planFechaInicio: hoy, planDuracionDias: 15));
        e.Catalogo.Agregar(CrearServicioActivo(TipoServicio.CATERING));
        var command = new ContratarServicioCommand(pacienteId, TipoServicio.CATERING, null);

        Assert.NotEqual(inicioAnterior, hoy);
        var ex = await Assert.ThrowsAsync<DomainException>(() => e.Handler.Handle(command, CancellationToken.None));

        Assert.Equal("C3_CATERING_YA_ACTIVO", ex.Error.Code);
        Assert.Equal(ErrorType.Conflict, ex.Error.Type);
    }

    [Fact]
    public async Task Handle_CateringAnteriorVencido_LoCompletaAntesDeContratarElNuevo()
    {
        var e = CrearEscenario();
        var pacienteId = Guid.NewGuid();
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var inicioAnterior = hoy.AddDays(-40);

        var servicioExistente = ServicioCatalogo.Crear(TipoServicio.CATERING, "Catering", Money.Crear(500m, "BOB"));
        var contratoAnterior = Contrato.Contratar(
            ContratoId.New(),
            PacienteId.From(pacienteId),
            servicioExistente.ObtenerServicioParaContrato(),
            Vigencia.Desde(inicioAnterior, 15), // vencido: terminó hace 25 días
            FacturaId.New());
        e.Contratos.Agregar(contratoAnterior);

        e.Pacientes.Agregar(CrearPaciente(pacienteId, tienePlanActivo: true, planFechaInicio: hoy, planDuracionDias: 15));
        e.Catalogo.Agregar(CrearServicioActivo(TipoServicio.CATERING));
        var command = new ContratarServicioCommand(pacienteId, TipoServicio.CATERING, null);

        var resultado = await e.Handler.Handle(command, CancellationToken.None);

        Assert.Equal(EstadoContrato.COMPLETADO, contratoAnterior.Estado);
        Assert.Equal(2, e.Contratos.Contratos.Count);
        var nuevo = Assert.Single(e.Contratos.Contratos, c => c.Id == resultado.ContratoId);
        Assert.Equal(EstadoContrato.ACTIVO, nuevo.Estado);
    }

    [Fact]
    public async Task Handle_Catering_CopiaLaVigenciaDelPlanYNoDelCommand()
    {
        var e = CrearEscenario();
        var pacienteId = Guid.NewGuid();
        var fechaInicioPlan = new DateOnly(2026, 9, 7);
        e.Pacientes.Agregar(CrearPaciente(pacienteId, tienePlanActivo: true, planFechaInicio: fechaInicioPlan, planDuracionDias: 15));
        e.Catalogo.Agregar(CrearServicioActivo(TipoServicio.CATERING));

        var fechaInicioCliente = new DateOnly(2099, 1, 1);
        var command = new ContratarServicioCommand(pacienteId, TipoServicio.CATERING, fechaInicioCliente);

        var resultado = await e.Handler.Handle(command, CancellationToken.None);

        var contrato = e.Contratos.Contratos.Single(c => c.Id == resultado.ContratoId);
        Assert.Equal(fechaInicioPlan, contrato.Vigencia.FechaInicio);
        Assert.Equal(15, contrato.Vigencia.DuracionDias);
        Assert.Equal(fechaInicioPlan.AddDays(14), contrato.Vigencia.FechaFin);
    }

    [Fact]
    public async Task Handle_Catering_IgnoraLaFechaInicioEnviadaPorElCliente()
    {
        var e = CrearEscenario();
        var pacienteId = Guid.NewGuid();
        var fechaInicioPlan = new DateOnly(2026, 9, 7);
        e.Pacientes.Agregar(CrearPaciente(pacienteId, tienePlanActivo: true, planFechaInicio: fechaInicioPlan, planDuracionDias: 30));
        e.Catalogo.Agregar(CrearServicioActivo(TipoServicio.CATERING));

        var fechaInicioCliente = fechaInicioPlan.AddDays(100);
        var command = new ContratarServicioCommand(pacienteId, TipoServicio.CATERING, fechaInicioCliente);

        var resultado = await e.Handler.Handle(command, CancellationToken.None);

        var contrato = e.Contratos.Contratos.Single(c => c.Id == resultado.ContratoId);
        Assert.NotEqual(fechaInicioCliente, contrato.Vigencia.FechaInicio);
        Assert.Equal(fechaInicioPlan, contrato.Vigencia.FechaInicio);
    }

    [Fact]
    public async Task Handle_UnCambioDePrecioPosterior_NoAlteraElContratoYaEmitido()
    {
        var e = CrearEscenario();
        var pacienteId = Guid.NewGuid();
        e.Pacientes.Agregar(CrearPaciente(pacienteId));
        var servicio = CrearServicioActivo(TipoServicio.EVALUACION, precio: 200m);
        e.Catalogo.Agregar(servicio);
        var command = new ContratarServicioCommand(pacienteId, TipoServicio.EVALUACION, null);

        var resultado = await e.Handler.Handle(command, CancellationToken.None);
        var contrato = e.Contratos.Contratos.Single(c => c.Id == resultado.ContratoId);
        Assert.Equal(200m, contrato.Servicio.Precio.Monto);

        // RN-26: un cambio de precio posterior en el catálogo no altera el contrato ya emitido.
        servicio.ActualizarPrecio(Money.Crear(999m, "BOB"));

        Assert.Equal(200m, contrato.Servicio.Precio.Monto);
    }

    [Fact]
    public async Task Handle_HappyPath_EmiteFacturaYContratoEnElMismoCommit()
    {
        var e = CrearEscenario();
        var pacienteId = Guid.NewGuid();
        e.Pacientes.Agregar(CrearPaciente(pacienteId));
        e.Catalogo.Agregar(CrearServicioActivo(TipoServicio.EVALUACION));
        var command = new ContratarServicioCommand(pacienteId, TipoServicio.EVALUACION, null);

        var resultado = await e.Handler.Handle(command, CancellationToken.None);

        Assert.Equal(1, e.UnitOfWork.Commits);
        Assert.Single(e.Contratos.Contratos);
        Assert.Single(e.Facturas.Facturas);
        var contrato = e.Contratos.Contratos.Single();
        var factura = e.Facturas.Facturas.Single();
        Assert.Equal(contrato.FacturaId.Value, factura.Id);
        Assert.Equal(resultado.ContratoId, contrato.Id);
        Assert.Equal(resultado.FacturaId, factura.Id);
    }

    [Fact]
    public async Task Handle_ElTotalUsaElPorcentajeConfiguradoEnFacturacionOptions_NoUnTrecePorCientoFijo()
    {
        var e = CrearEscenario(porcentajeImpuesto: 21m);
        var pacienteId = Guid.NewGuid();
        e.Pacientes.Agregar(CrearPaciente(pacienteId));
        e.Catalogo.Agregar(CrearServicioActivo(TipoServicio.EVALUACION, precio: 100m));
        var command = new ContratarServicioCommand(pacienteId, TipoServicio.EVALUACION, null);

        await e.Handler.Handle(command, CancellationToken.None);

        var factura = e.Facturas.Facturas.Single();
        Assert.Equal(100m, factura.Subtotal.Monto);
        Assert.Equal(21m, factura.Impuesto.Monto);
        Assert.Equal(121m, factura.Total.Monto);
    }
}
