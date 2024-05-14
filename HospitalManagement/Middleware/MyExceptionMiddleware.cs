using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Serilog;
using System.Threading.Tasks;

namespace HospitalManagement.Middleware
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class MyExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public MyExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An unhandled exception occurred.");
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await httpContext.Response.WriteAsync("An error occurred. Please try again later.");
            }
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class MyExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseMyExceptionMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<MyExceptionMiddleware>();
        }
    }
}
