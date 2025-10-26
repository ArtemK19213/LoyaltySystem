using System.Net;
using System.Text.Json;

namespace LoyaltySystem.API.Infrastructure.Middleware;

public class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
{
    public async Task Invoke(HttpContext ctx)
    {
        try { await next(ctx); }
        catch (UnauthorizedAccessException ex) { await Write(ctx, HttpStatusCode.Unauthorized, ex.Message); }
        catch (InvalidOperationException ex)   { await Write(ctx, HttpStatusCode.BadRequest, ex.Message); }
        catch (Exception ex) { logger.LogError(ex, "Unhandled"); await Write(ctx, HttpStatusCode.InternalServerError, "Internal Server Error"); }
    }

    private static Task Write(HttpContext c, HttpStatusCode code, string msg)
    { c.Response.ContentType = "application/json"; c.Response.StatusCode = (int)code; return c.Response.WriteAsync(JsonSerializer.Serialize(new { error = msg })); }
}
