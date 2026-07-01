using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.WebApi.Common;
using FluentValidation;
using System.Text.Json;

namespace Ambev.DeveloperEvaluation.WebApi.Middleware;

/// <summary>
/// Global exception handler that converts unhandled exceptions into the standard
/// <c>{ "type", "error", "detail" }</c> error response with an appropriate HTTP status code.
/// </summary>
public class ErrorHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
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
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, type, error, detail) = exception switch
        {
            ValidationException validation => (
                StatusCodes.Status400BadRequest,
                "ValidationError",
                "Invalid input data",
                string.Join("; ", validation.Errors.Select(e => e.ErrorMessage))),

            DomainException domain => (
                StatusCodes.Status400BadRequest,
                "DomainRuleViolation",
                "Business rule violation",
                domain.Message),

            KeyNotFoundException notFound => (
                StatusCodes.Status404NotFound,
                "ResourceNotFound",
                "Resource not found",
                notFound.Message),

            InvalidOperationException invalid => (
                StatusCodes.Status409Conflict,
                "ConflictError",
                "The request could not be completed",
                invalid.Message),

            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                "AuthenticationError",
                "Authentication failed",
                "The request is not authorized."),

            _ => (
                StatusCodes.Status500InternalServerError,
                "InternalServerError",
                "An unexpected error occurred",
                "An unexpected error occurred while processing your request.")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Unhandled exception while processing {Path}", context.Request.Path);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var payload = new ErrorResponse { Type = type, Error = error, Detail = detail };
        return context.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOptions));
    }
}
