using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TapForPerksAPI.DbContexts;
using TapForPerksAPI.Repositories;
using TapForPerksAPI.Services;

var builder = WebApplication.CreateBuilder(args);

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
}

// Configure CORS for all environments (not just Development)
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
if (allowedOrigins != null && allowedOrigins.Length > 0)
{
    // inMemoryLogger.Log($"CORS: Configuring allowed origins: {string.Join(", ", allowedOrigins)}");

    app.UseCors(corsBuilder =>
    corsBuilder
   .WithOrigins(allowedOrigins)
      .AllowAnyMethod()
     .AllowAnyHeader()
   .AllowCredentials());
}
else
{
   // inMemoryLogger.Log("CORS: No allowed origins configured");
}

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
