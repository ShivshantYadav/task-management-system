using System.Net;
using System.Text.Json;
using TaskManagement.Application.Common;

namespace TaskManagement.API.Middleware
{
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
                var (status, message) = ex switch
                {
                    NotFoundException => (HttpStatusCode.NotFound, ex.Message),
                    ForbiddenException => (HttpStatusCode.Forbidden, ex.Message),
                    BadRequestException => (HttpStatusCode.BadRequest, ex.Message),
                    ConflictException => (HttpStatusCode.Conflict, ex.Message),
                    _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
                };

                if (status == HttpStatusCode.InternalServerError)
                    _logger.LogError(ex, "Unhandled exception");

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)status;

                var payload = JsonSerializer.Serialize(new { message });
                await context.Response.WriteAsync(payload);
            }
        }
    }
}
