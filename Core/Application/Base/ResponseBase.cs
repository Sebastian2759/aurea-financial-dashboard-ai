using System.Net;

namespace Application.Base;

/// <summary>
/// Respuesta estándar de todos los use cases.
/// Usa los factory methods Ok/Created/NotFound/Conflict/BadRequest.
/// </summary>
public sealed class ResponseBase<T>
{
    public bool IsSuccess { get; private init; }
    public HttpStatusCode StatusCode { get; private init; }
    public string Message { get; private init; } = string.Empty;
    public T? Data { get; private init; }
    public IReadOnlyList<string>? Errors { get; private init; }

    // --- Factory methods ---

    public static ResponseBase<T> Ok(T data, string message = "Operación exitosa.")
        => new() { IsSuccess = true, StatusCode = HttpStatusCode.OK, Message = message, Data = data };

    public static ResponseBase<T> Created(T data, string message = "Recurso creado.")
        => new() { IsSuccess = true, StatusCode = HttpStatusCode.Created, Message = message, Data = data };

    public static ResponseBase<T> NotFound(string message = "Recurso no encontrado.")
        => new() { IsSuccess = false, StatusCode = HttpStatusCode.NotFound, Message = message };

    public static ResponseBase<T> Conflict(string message)
        => new() { IsSuccess = false, StatusCode = HttpStatusCode.Conflict, Message = message };

    public static ResponseBase<T> BadRequest(string message, IReadOnlyList<string>? errors = null)
        => new() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest, Message = message, Errors = errors };

    public static ResponseBase<T> UnprocessableEntity(string message, IReadOnlyList<string>? errors = null)
        => new() { IsSuccess = false, StatusCode = HttpStatusCode.UnprocessableContent, Message = message, Errors = errors };
}
