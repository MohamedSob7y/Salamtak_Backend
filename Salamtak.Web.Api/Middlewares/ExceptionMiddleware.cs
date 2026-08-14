using Salamtak.services.Exceptions;
using Salamtak.Shared.Responses;
using System.Net;
using System.Text.Json;

namespace Salamtak.Web.Api.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IWebHostEnvironment _environment;

        public ExceptionMiddleware(RequestDelegate next, IWebHostEnvironment environment)
        {
            _next = next;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (AppValidationException ex)
            {
                context.Response.StatusCode = ex.StatusCode;
                context.Response.ContentType = "application/json";

                var response = new ErrorResponse
                {
                    StatusCode = ex.StatusCode,
                    Message = ex.Message,
                    Errors = ex.Errors
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
            catch (BaseAppException ex)
            {
                context.Response.StatusCode = ex.StatusCode;
                context.Response.ContentType = "application/json";

                var response = new ErrorResponse
                {
                    StatusCode = ex.StatusCode,
                    Message = ex.Message
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                var response = new ErrorResponse
                {
                    StatusCode = 500,
                    Message = _environment.IsDevelopment()
                        ? ex.Message
                        : "An unexpected error occurred."
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        }
    }
}
