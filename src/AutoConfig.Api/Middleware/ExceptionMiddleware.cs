using System.Text.Json;
using AutoConfig.Core.Exceptions;

namespace AutoConfig.Api.Middleware;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await next(ctx);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await WriteErrorAsync(ctx, ex);
        }
    }

    private static Task WriteErrorAsync(HttpContext ctx, Exception ex)
    {
        var (status, message) = ex switch
        {
            AppException app => (app.StatusCode, app.Message),
            _ => (500, "Si è verificato un errore interno.")
        };

        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "application/json";

        return ctx.Response.WriteAsync(JsonSerializer.Serialize(new { error = message }));
    }
}
