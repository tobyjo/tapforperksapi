using AutoMapper;
using Azure.Identity;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using SaveForPerksAPI.DbContexts;
using SaveForPerksAPI.Repositories;
using SaveForPerksAPI.Services;
using Serilog;
using Serilog.Events;
using System.Diagnostics;
using System.Runtime.InteropServices;


// Configure Serilog FIRST (before building the app)
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "TapForPerksAPI")
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/tapforperks-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting TapForPerksAPI");

    var builder = WebApplication.CreateBuilder(args);

    // Use Serilog for all logging
    builder.Host.UseSerilog();

    // Configure Kestrel to listen on both HTTP and HTTPS
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenLocalhost(5143); // HTTP
        /*
        options.ListenLocalhost(7040, listenOptions =>
        {
            listenOptions.UseHttps(); // HTTPS
        });
        */
    });

    // Configure Azure Key Vault integration
    var keyVaultName = builder.Configuration["Azure:KeyVaultName"];
    if (!string.IsNullOrEmpty(keyVaultName))
    {
        try
        {
            Log.Information("STARTUP: Attempting to connect to Key Vault: {KeyVaultUri}", keyVaultName);
            var keyVaultUri = new Uri($"https://{keyVaultName}.vault.azure.net/");
            builder.Configuration.AddAzureKeyVault(keyVaultUri, new DefaultAzureCredential());

        }
        catch (Exception ex)
        {
            // Add logging for Key Vault connection failure
            Log.Error(ex, "STARTUP: Failed to connect to Key Vault: {KeyVaultName}", keyVaultName);
        }
    }
    else
    {
        Log.Information("STARTUP: No Key Vault configured");
    }


    // Add services to the container.
    builder.Services.AddDbContext<TapForPerksContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ISaveForPerksRepository, SaveForPerksRepository>();
builder.Services.AddScoped<IQrCodeService, QrCodeService>();
builder.Services.AddScoped<IRewardService, RewardService>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


// AutoMapper license key now loaded from configuration
var autoMapperLicenseKey = builder.Configuration["AutoMapper:LicenseKey"];
builder.Services.AddAutoMapper(cfg => cfg.LicenseKey = autoMapperLicenseKey, AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    var mapper = app.Services.GetRequiredService<IMapper>();
    mapper.ConfigurationProvider.AssertConfigurationIsValid();

    app.MapOpenApi();
        
    // Development: Show detailed errors for debugging
    app.UseDeveloperExceptionPage();
}
else
{
    // Production: Use global exception handler
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";

            var exceptionHandlerPathFeature = 
                context.Features.Get<IExceptionHandlerPathFeature>();
            var exception = exceptionHandlerPathFeature?.Error;

            // Log with Serilog (structured logging)
            Log.Error(exception, 
                "Unhandled exception. Path: {Path}, Method: {Method}, TraceId: {TraceId}", 
                context.Request.Path.Value, 
                context.Request.Method,
                Activity.Current?.Id ?? context.TraceIdentifier);

            // Return generic error to client (don't expose internal details)
            await context.Response.WriteAsJsonAsync(new
            {
                error = "An unexpected error occurred. Please try again later.",
                traceId = Activity.Current?.Id ?? context.TraceIdentifier
            });
        });
    });
        
    app.UseHsts(); // Add HTTP Strict Transport Security
}

    // Configure CORS for all environments (not just Development)
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
    if (allowedOrigins != null && allowedOrigins.Length > 0)
    {
        Log.Information("CORS: Configuring allowed origins: {AllowedOrigins}", string.Join(", ", allowedOrigins));

        app.UseCors(corsBuilder =>
        corsBuilder
       .WithOrigins(allowedOrigins)
          .AllowAnyMethod()
         .AllowAnyHeader()
       .AllowCredentials());
    }
    else
    {
       Log.Warning("CORS: No allowed origins configured");
    }

    // app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    Log.Information("TapForPerksAPI started successfully on {Environment}", app.Environment.EnvironmentName);
    
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start");
    throw;
}
finally
{
    Log.Information("Shutting down TapForPerksAPI");
    Log.CloseAndFlush();
}
