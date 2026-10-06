using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Scalar.AspNetCore;
using TaskManagement.Api;
using TaskManagement.Application;
using TaskManagement.Domain.Exceptions;
using TaskManagement.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment.EnvironmentName);

var app = builder.Build();

var apiDocsEnabled = app.Environment.IsDevelopment()
    || app.Environment.IsEnvironment("Testing")
    || string.Equals(app.Configuration["ApiDocs:Enabled"], "true", StringComparison.OrdinalIgnoreCase);

if (apiDocsEnabled)
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Task Management API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

        var (statusCode, title, errors) = exception switch
        {
            TaskNotFoundException => (StatusCodes.Status404NotFound, "Task not found", null),
            InvalidStatusTransitionException ex => (StatusCodes.Status400BadRequest, "Invalid status transition", new[] { ex.Message }),
            ValidationException ex => (StatusCodes.Status400BadRequest, "Validation failed", ex.Errors.Select(error => error.ErrorMessage).ToArray()),
            ArgumentException ex => (StatusCodes.Status400BadRequest, "Invalid request", new[] { ex.Message }),
            _ => (StatusCodes.Status500InternalServerError, "Unexpected error", null)
        };

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(new
        {
            title,
            status = statusCode,
            errors
        });
    });
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
