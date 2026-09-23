using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using videoGameCharacterApi.Exceptions;

namespace videoGameCharacterApi.Middleware;

/// <summary>
/// Catches every unhandled exception and returns a clean ProblemDetails JSON response
/// instead of a stack trace.
/// </summary>
public class GlobalExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Not found"),
            BadRequestException => (StatusCodes.Status400BadRequest, "Bad request"),
            _ => (StatusCodes.Status500InternalServerError, "Server error")
        };

        if (status == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled exception on {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
        else
            logger.LogWarning("{Title}: {Message}", title, exception.Message);

        httpContext.Response.StatusCode = status;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                // Never leak internal error details to the client.
                Detail = status == StatusCodes.Status500InternalServerError
                    ? "An unexpected error occurred."
                    : exception.Message
            }
        });
    }
}
