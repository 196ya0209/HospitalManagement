using Serilog;

namespace HospitalManagement.Middleware
{
    public class GlobalExceptionHandlerMiddleware : IMiddleware
    {
        private readonly RequestDelegate _next;        

        public GlobalExceptionHandlerMiddleware(RequestDelegate next)
        {
            _next = next;            
        }
        
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An unhandled exception occurred.");
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync("An error occurred. Please try again later.");
            }
        }
    }

    public static class MyMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GlobalExceptionHandlerMiddleware>();
        }
    }
}
