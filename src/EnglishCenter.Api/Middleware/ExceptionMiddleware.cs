using System.Net;
using System.Text.Json;
using EnglishCenter.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenter.Api.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        var (status, title) = exception switch
        {
            NotFoundException => (HttpStatusCode.NotFound, "Không tìm thấy"),
            ConflictException => (HttpStatusCode.Conflict, "Xung đột dữ liệu"),
            BusinessException be => ((HttpStatusCode)be.StatusCode, "Lỗi nghiệp vụ"),
            _ => (HttpStatusCode.InternalServerError, "Lỗi hệ thống"),
        };

        if (status == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Unhandled exception");

        var problem = new ProblemDetails
        {
            Status = (int)status,
            Title = title,
            Detail = exception.Message,
            Type = $"https://httpstatuses.com/{(int)status}",
        };
        problem.Extensions["traceId"] = context.TraceIdentifier;

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)status;
        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}
