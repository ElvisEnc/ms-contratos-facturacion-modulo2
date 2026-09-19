using System.Text.Json.Serialization;
using Contratos.Application.Configuration;
using Contratos.Application.Contratos.ContratarServicio;
using Contratos.Application.Ports;
using Contratos.Infrastructure.Messaging;
using Contratos.Infrastructure.Persistence;
using Contratos.Infrastructure.Persistence.Repositories;
using Contratos.WebApi.Middleware;
using Joseco.DDD.Core.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    // §12.11 del maestro: enums como string en mayúsculas, propiedades en
    // camelCase. Sin el converter, un cuerpo como {"tipo":"CATERING"} se
    // rechaza con 400 antes de llegar al handler.
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ContratosDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ContratosDatabase")));

builder.Services.AddScoped<IContratoRepository, ContratoRepository>();
builder.Services.AddScoped<IFacturaRepository, FacturaRepository>();
builder.Services.AddScoped<IControlAdicionalRepository, ControlAdicionalRepository>();
builder.Services.AddScoped<IServicioCatalogoRepository, ServicioCatalogoRepository>();
builder.Services.AddScoped<IPacienteConocidoStore, PacienteConocidoStore>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IIntegrationEventPublisher, LoggingIntegrationEventPublisher>();

// Los handlers inyectan FacturacionOptions directamente, no IOptions<T>: se
// liga la sección de configuración y además se expone la instancia resuelta.
builder.Services.Configure<FacturacionOptions>(
    builder.Configuration.GetSection(FacturacionOptions.SeccionConfiguracion));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<FacturacionOptions>>().Value);

// Doble escaneo (§17 fase 5 del DESIGN): los command handlers viven en
// Contratos.Application, los cinco query handlers en Contratos.Infrastructure
// (§12.8 del maestro). Sin el segundo assembly, toda consulta responde
// "No handler registered".
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
    typeof(ContratarServicioCommand).Assembly,
    typeof(ContratosDbContext).Assembly));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // §12.12 del maestro: migraciones automáticas solo en Development,
    // resolviendo el DbContext desde un scope antes del pipeline HTTP.
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<ContratosDbContext>().Database.Migrate();
}

app.UseHttpsRedirection();

// Antes de MapControllers: traduce Error.Type a HTTP una sola vez (§12.1 del maestro).
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

app.Run();
