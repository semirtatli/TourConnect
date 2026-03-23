using System.Net;
using System.Text.Json;

namespace TourConnect.Prototype.Middleware;

// ExceptionHandlingMiddleware: tüm unhandled exception'ları yakalar.
// Controller'lardaki NotFound(), BadRequest() gibi explicit dönüşler buraya düşmez —
// sadece try/catch ile yakalanmayan exception'lar buraya gelir.
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    // RequestDelegate _next → pipeline'daki bir sonraki middleware veya controller
    // ILogger → hataları logluyoruz (prod'da Sentry/Datadog gibi araçlara gönderilir)
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Bir sonraki middleware'i çalıştır.
            // Hata fırlatılmazsa buradan direkt devam edilir.
            await _next(context);
        }
        catch (Exception ex)
        {
            // Beklenmedik bir hata oluştu — logla ve düzgün yanıt döndür.
            _logger.LogError(ex, "Beklenmedik hata: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // HTTP durum kodunu ve hata mesajını belirle.
        // İleride özel exception türleri eklenebilir:
        // catch (NotFoundException ex) → 404
        // catch (ConflictException ex) → 409
        var (statusCode, title) = exception switch
        {
            KeyNotFoundException => (HttpStatusCode.NotFound, "Kaynak bulunamadı"),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Yetkisiz erişim"),
            _ => (HttpStatusCode.InternalServerError, "Sunucu hatası")
        };

        // Problem Details (RFC 7807) formatı:
        // Tüm HTTP API'lerin hata döndürmek için kullandığı standart format.
        var problemDetails = new
        {
            title,
            detail = exception.Message,
            status = (int)statusCode
        };

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(problemDetails));
    }
}
