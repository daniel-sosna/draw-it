namespace Draw.it.Server.Exceptions;

public class ExceptionHandler
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandler> _logger;

    public ExceptionHandler(RequestDelegate next, ILogger<ExceptionHandler> logger)
    {
        _next = next;
        _logger = logger;
    }

    // Catches exceptions thrown in code and provides useful information in the response
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context); // continue request
        }
        catch (AppException ex)
        {
            context.Response.StatusCode = (int) ex.Status;
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new { error = "An unexpected error occurred." });
        }
    }
}