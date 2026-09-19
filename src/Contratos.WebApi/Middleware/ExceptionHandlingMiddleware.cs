using System.Text.Json;
using Joseco.DDD.Core.Results;

namespace Contratos.WebApi.Middleware;

// §12.1 del maestro: un único punto que traduce Error.Type a HTTP, en vez de
// repetir la comprobación del resultado en cada controller. Sin catch-all:
// cualquier otra excepción cae al manejo por defecto de ASP.NET Core.
public sealed class ExceptionHandlingMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (DomainException ex)
        {
            var status = ex.Error.Type switch
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.Failure => StatusCodes.Status400BadRequest,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.Problem => StatusCodes.Status422UnprocessableEntity,
                _ => StatusCodes.Status400BadRequest
            };

            await EscribirAsync(context, status, ex.Error.Code, ex.Error.Description);
        }
        catch (ArgumentException ex)
        {
            // Red de seguridad de INC-S2: Entity(Guid.Empty) del núcleo lanza
            // ArgumentException, no DomainException. Sin esta rama saldría
            // como 500 sin cuerpo. No sustituye lanzar DomainException desde
            // código propio.
            await EscribirAsync(context, StatusCodes.Status400BadRequest, "ARGUMENTO_INVALIDO", ex.Message);
        }
    }

    private static Task EscribirAsync(HttpContext context, int statusCode, string codigo, string mensaje)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        return context.Response.WriteAsync(JsonSerializer.Serialize(new { codigo, mensaje }));
    }
}
