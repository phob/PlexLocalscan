using PlexLocalScan.Api.Endpoints;
using PlexLocalScan.SignalR.Hubs;
using Scalar.AspNetCore;
using Serilog;
using Hangfire;

namespace PlexLocalScan.Api.ServiceCollection;

public static class Middleware
{
    public static WebApplication AddMiddleware(this WebApplication app)
    {
        // Configure CORS
        app.UseCors();

        // Configure the HTTP request pipeline
        app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
            if (error != null)
            {
                var ex = error.Error;
                Log.Error(ex, "An unhandled exception occurred");
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "An internal server error occurred.",
                    details = app.Environment.IsDevelopment() ? ex.Message : null
                });
            }
        }));

        // Add middleware in the correct order
        app.UseRouting();

        // Map endpoints
        app.MapControllers();
        app.MapHub<ContextHub>(ContextHub.Route);
        app.MapApiEndpoints();

        app.MapOpenApi();
        app.MapScalarApiReference(options => options.Theme = ScalarTheme.Mars);
        app.MapGet("/", () => Results.Redirect("/scalar/v1"));

        app.UseHangfireDashboard("/hangfire");

        return app;
    }
} 