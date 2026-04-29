using Arboon.API.Middleware;
using Arboon.Application;
using Arboon.Infrastructure;
using Arboon.Infrastructure.Data;
using Arboon.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;

// ──────────────────────────────────────────────
// Bootstrap Serilog early for startup logging
// ──────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("🚀 Starting Arboon API...");

    var builder = WebApplication.CreateBuilder(args);

    // ──────────────────────────────────────────
    // Serilog — structured logging
    // ──────────────────────────────────────────
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    // ──────────────────────────────────────────
    // Application & Infrastructure DI
    // ──────────────────────────────────────────
    builder.Services.AddApplicationServices();
    builder.Services.AddInfrastructureServices(builder.Configuration);

    // ──────────────────────────────────────────
    // Controllers + JSON options
    // ──────────────────────────────────────────
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy =
                System.Text.Json.JsonNamingPolicy.SnakeCaseLower;
        });

    // ──────────────────────────────────────────
    // HTTP Logging (to log request/response data)
    // ──────────────────────────────────────────
    builder.Services.AddHttpLogging(logging =>
    {
        logging.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
        logging.RequestBodyLogLimit = 4096;
        logging.ResponseBodyLogLimit = 4096;
    });

    // ──────────────────────────────────────────
    // Swagger / OpenAPI
    // ──────────────────────────────────────────
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Arboon API",
            Version = "v1",
            Description = "عَرْبُون — Micro-Escrow Infrastructure for Freelancers & Small Businesses",
            Contact = new OpenApiContact
            {
                Name = "Arboon Team",
                Url = new Uri("https://arboon.app")
            }
        });

        // JWT Authentication in Swagger
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "أدخل JWT token: Bearer {token}"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    // ──────────────────────────────────────────
    // CORS
    // ──────────────────────────────────────────
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            var frontendUrl = builder.Configuration["AppSettings:FrontendUrl"] ?? "http://localhost:3000";
            policy
                .WithOrigins(frontendUrl, "https://arboon.app")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
    });

    // ──────────────────────────────────────────
    // Authorization
    // ──────────────────────────────────────────
    builder.Services.AddAuthorization();

    var app = builder.Build();

    // ──────────────────────────────────────────
    // Middleware pipeline
    // ──────────────────────────────────────────

    // Global exception handling (first in pipeline)
    app.UseMiddleware<GlobalExceptionMiddleware>();

    // Http Logging (to log request/response data locally)
    if (app.Environment.IsDevelopment())
    {
        app.UseHttpLogging();
    }

    // Serilog request logging
    app.UseSerilogRequestLogging();

    // Swagger (all environments for now)
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Arboon API v1");
        options.RoutePrefix = "swagger";
    });

    app.UseHttpsRedirection();
    app.UseCors("AllowFrontend");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    // ──────────────────────────────────────────
    // Database migration & seeding (Development)
    // ──────────────────────────────────────────
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            logger.LogInformation("Applying database migrations...");
            await dbContext.Database.MigrateAsync();
            logger.LogInformation("✅ Database migrations applied successfully.");

            if (app.Environment.IsDevelopment())
            {
                await DataSeeder.SeedAsync(dbContext, logger);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "⚠️ Could not apply migrations (database may not be available). " +
                "Run 'dotnet ef database update' manually.");
        }
    }

    Log.Information("✅ Arboon API started successfully on {Urls}", string.Join(", ", app.Urls));
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ Arboon API terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
