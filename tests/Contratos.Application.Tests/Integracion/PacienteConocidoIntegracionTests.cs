using Contratos.Application.Integracion.ProcesarPacienteRegistrado;
using Contratos.Application.Integracion.ProcesarPlanAsignado;
using Contratos.Application.Tests.Fakes;
using Contratos.Domain.Shared;

namespace Contratos.Application.Tests.Integracion;

public class PacienteConocidoIntegracionTests
{
    [Fact]
    public async Task ProcesarPacienteRegistrado_LuegoProcesarPlanAsignado_ConservaAmbosGruposDeCampos()
    {
        var store = new FakePacienteConocidoStore();
        var handlerPaciente = new ProcesarPacienteRegistradoCommandHandler(store);
        var handlerPlan = new ProcesarPlanAsignadoCommandHandler(store);
        var pacienteId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var fechaInicio = new DateOnly(2026, 9, 7);

        await handlerPaciente.Handle(
            new ProcesarPacienteRegistradoCommand(pacienteId, "Ana Pérez", "1234567"), CancellationToken.None);
        await handlerPlan.Handle(
            new ProcesarPlanAsignadoCommand(pacienteId, planId, fechaInicio, 15), CancellationToken.None);

        Assert.Equal(1, store.Cantidad);
        var paciente = (await store.GetByIdAsync(PacienteId.From(pacienteId)))!;
        Assert.Equal("Ana Pérez", paciente.Nombre);
        Assert.Equal("1234567", paciente.NroIdentificacion);
        Assert.Equal(planId, paciente.PlanId);
        Assert.True(paciente.TienePlanActivo);
        Assert.Equal(fechaInicio, paciente.PlanFechaInicio);
        Assert.Equal(15, paciente.PlanDuracionDias);
    }

    [Fact]
    public async Task ProcesarPlanAsignado_ConPacienteAunNoRegistrado_CreaLaReplicaConIdentidadVacia()
    {
        var store = new FakePacienteConocidoStore();
        var handlerPlan = new ProcesarPlanAsignadoCommandHandler(store);
        var pacienteId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var fechaInicio = new DateOnly(2026, 9, 7);

        await handlerPlan.Handle(new ProcesarPlanAsignadoCommand(pacienteId, planId, fechaInicio, 30), CancellationToken.None);

        Assert.Equal(1, store.Cantidad);
        var paciente = (await store.GetByIdAsync(PacienteId.From(pacienteId)))!;
        Assert.Equal(string.Empty, paciente.Nombre);
        Assert.Equal(string.Empty, paciente.NroIdentificacion);
        Assert.Equal(planId, paciente.PlanId);
        Assert.True(paciente.TienePlanActivo);
        Assert.Equal(fechaInicio, paciente.PlanFechaInicio);
        Assert.Equal(30, paciente.PlanDuracionDias);
    }

    [Fact]
    public async Task ReprocesarPacienteRegistrado_NoDuplicaLaReplica()
    {
        var store = new FakePacienteConocidoStore();
        var handler = new ProcesarPacienteRegistradoCommandHandler(store);
        var pacienteId = Guid.NewGuid();
        var command = new ProcesarPacienteRegistradoCommand(pacienteId, "Ana Pérez", "1234567");

        await handler.Handle(command, CancellationToken.None);
        await handler.Handle(command, CancellationToken.None);

        Assert.Equal(1, store.Cantidad);
    }

    [Fact]
    public async Task ReprocesarPlanAsignado_NoDuplicaLaReplica()
    {
        var store = new FakePacienteConocidoStore();
        var handler = new ProcesarPlanAsignadoCommandHandler(store);
        var pacienteId = Guid.NewGuid();
        var command = new ProcesarPlanAsignadoCommand(pacienteId, Guid.NewGuid(), new DateOnly(2026, 9, 7), 15);

        await handler.Handle(command, CancellationToken.None);
        await handler.Handle(command, CancellationToken.None);

        Assert.Equal(1, store.Cantidad);
    }
}
