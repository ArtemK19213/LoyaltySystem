using System.Net;

namespace LoyaltySystem.API.Infrastructure.Middleware;

// Простое исключение с кодом статуса для удобных ошибок API
public class AppException : Exception
{
    public HttpStatusCode StatusCode { get; }

    public AppException(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        : base(message) => StatusCode = statusCode;
}
