using System.Net;
using System.Text.Json;

namespace TourConnect.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Beklenmedik hata: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // KeyNotFoundException   → 404 (bulunamadı)
        // InvalidOperationException → 400 (geçersiz işlem: slot yok, deal dolu vb.)
        // Diğerleri             → 500 (beklenmedik hata)
        var (statusCode, title) = exception switch
        {
            KeyNotFoundException => (HttpStatusCode.NotFound, "Kaynak bulunamadı"),
            InvalidOperationException => (HttpStatusCode.BadRequest, "Geçersiz işlem"),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Yetkisiz erişim"),
            _ => (HttpStatusCode.InternalServerError, "Sunucu hatası")
        };

        var problemDetails = new
        {
            title,
            detail = exception.Message,
            status = (int)statusCode
        };

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
    }
}
