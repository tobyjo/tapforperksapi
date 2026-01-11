using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TapForPerksAPI.DbContexts;
using TapForPerksAPI.Repositories;
using TapForPerksAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<TapForPerksContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ISaveForPerksRepository, SaveForPerksRepository>();
builder.Services.AddScoped<IQrCodeService, QrCodeService>();

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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
