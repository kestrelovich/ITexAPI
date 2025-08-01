using FluentValidation;
using ITexAPI.Models.DTOs.Common;
using System.Text.Json;

namespace ITexAPI.Middlewares
{
    public class ValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public ValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException validationException)
            {
                await HandleValidationExceptionAsync(context, validationException);
            }
        }

        private static async Task HandleValidationExceptionAsync(HttpContext context, ValidationException validationException)
        {
            context.Response.StatusCode = 400;
            context.Response.ContentType = "application/json";

            var errors = validationException.Errors.Select(e => e.ErrorMessage).ToList();
            var response = ApiResponse<object>.ErrorResponse("Validation failed", errors);

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }
}