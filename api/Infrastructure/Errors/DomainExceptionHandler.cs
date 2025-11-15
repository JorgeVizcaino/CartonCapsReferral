using Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace api.Infrastructure.Errors
{
    public sealed class DomainExceptionHandler : IExceptionHandler
    {
        private readonly IProblemDetailsService problemDetailsService;
        private readonly ILogger<DomainExceptionHandler> logger;

        public DomainExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<DomainExceptionHandler> logger)
        {
            this.problemDetailsService = problemDetailsService;
            this.logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var (statusCode, errorCode, title) = exception switch
            {
                DomainException domainException => ((int)domainException.StatusCode, domainException.ErrorCode, domainException.Message),
                _ => (StatusCodes.Status500InternalServerError, "server_error", "An unexpected error occurred.")

            };

            if (statusCode >= StatusCodes.Status500InternalServerError)
            {
                logger.LogError(exception, "Unhandled exception");
            }
            else
            {
                logger.LogWarning(exception, "Domain exception");

            }

            httpContext.Response.StatusCode = statusCode;           

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = exception.Message,
                Type = errorCode,
                Instance = httpContext.Request.Path
            };

            await problemDetailsService.WriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problemDetails,
                Exception = exception
            });

            return true;
        }
    }
}
