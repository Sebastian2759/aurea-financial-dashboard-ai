using Domain.Base;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Domain.Security;
using Microsoft.EntityFrameworkCore;
using Domain.Contracts.Diagnostics;
using Domain.Diagnostics;

namespace Api.Handlers;

/// <summary>
/// Manejador global de excepciones (.NET 8+ IExceptionHandler).
/// Captura excepciones no controladas y retorna ProblemDetails estándar RFC 9457.
///
/// Registro en Program.cs:
///   builder.Services.AddExceptionHandler&lt;GlobalExceptionHandler&gt;();
///   builder.Services.AddProblemDetails();
///   app.UseExceptionHandler();
/// </summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, ISystemEventSink events)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken ct)
    {
        var (status, title, errors) = exception switch
        {
            ValidationException ve => (
                HttpStatusCode.BadRequest,
                "Errores de validación.",
                ve.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}").ToArray()),

            DomainException de => (
                HttpStatusCode.UnprocessableContent,
                de.Message,
                de.Errors.Select(kv => $"{kv.Key}: {kv.Value}").ToArray()),

            AccessDeniedException => (HttpStatusCode.Forbidden, "No tienes permiso para esta operación.", Array.Empty<string>()),
            DemoDisabledException => (HttpStatusCode.NotFound, "Recurso no disponible.", Array.Empty<string>()),
            MarketUnavailableException => (HttpStatusCode.ServiceUnavailable, "Datos de mercado temporalmente no disponibles.", Array.Empty<string>()),
            DbUpdateException when exception.InnerException?.Message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) == true || exception.InnerException?.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase) == true => (HttpStatusCode.Conflict, "El recurso ya existe.", Array.Empty<string>()),
            _ => (HttpStatusCode.InternalServerError, "Error interno del servidor.", Array.Empty<string>())
        };

        if (status == HttpStatusCode.InternalServerError)
        {
            events.Record(SystemEventKind.RequestFailed);
            logger.LogError(exception, "Excepción no controlada: {Message}", exception.Message);
        }

        var problem = new ProblemDetails
        {
            Status = (int)status,
            Title = title,
            Type = $"https://httpstatuses.com/{(int)status}",
        };

        if (errors.Length > 0)
            problem.Extensions["errors"] = errors;

        context.Response.StatusCode = (int)status;
        await context.Response.WriteAsJsonAsync(problem, ct);
        return true;
    }
}
