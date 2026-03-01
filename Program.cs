using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using UserManagementAPI.Data;
using UserManagementAPI.Services;
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);

// Integrar Azure Key Vault si se proporciona el nombre en configuración o variable de entorno
var keyVaultName = builder.Configuration["KeyVaultName"]; // configurar en appsettings o variable de entorno
if (!string.IsNullOrEmpty(keyVaultName))
{
    var kvUri = new Uri($"https://{keyVaultName}.vault.azure.net/");
    builder.Configuration.AddAzureKeyVault(kvUri, new DefaultAzureCredential());
}

//CONFIGURACIÓN DE SERVICIOS
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger + seguridad JWT en la UI (muestra el botón Authorize)
builder.Services.AddSwaggerGen(options =>
{
    // OperationFilter deshabilitado temporalmente para evitar cargar tipos
    // incompatibles en tiempo de ejecución. Rehabilitar solo si las
    // versiones de `Swashbuckle.AspNetCore` y
    // `Swashbuckle.AspNetCore.Filters` están alineadas.
    // options.OperationFilter<Swashbuckle.AspNetCore.Filters.SecurityRequirementsOperationFilter>();
});

builder.Services.AddOpenApi();    

// Servicio de autenticación
builder.Services.AddScoped<IAuthService, AuthService>();

// Base de Datos
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// AutoMapper
// Registrar AutoMapper usando la sobrecarga correcta: primero la acción de configuración y después los ensamblados
// Esto evita error de sobrecarga cuando la extensión espera (Action<IMapperConfigurationExpression>, params Assembly[])
builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(Program).Assembly), typeof(Program).Assembly);

var app = builder.Build();

//CONFIGURACIÓN DEL MIDDLEWARE

// Middleware personalizado para manejo global de excepciones (Debe ir al PRINCIPIO para capturar cualquier error)
app.UseMiddleware<UserManagementAPI.Helpers.ExceptionMiddleware>();

// Configuración de Swagger y OpenAPI (Solo en desarrollo)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "User Management API V1");
        c.RoutePrefix = "swagger"; // Acceso en https://localhost:7081/swagger
    });
    app.MapOpenApi();
}
// Redirección a HTTPS (Debe ir ANTES de app.UseAuthentication() y app.UseAuthorization())
app.UseHttpsRedirection();

// Autenticación y Autorización (Debe ir ANTES de MapControllers)
app.UseAuthentication();
// app.UseAuthorization() debe ir DESPUÉS de app.UseAuthentication()
app.UseAuthorization();
// MapControllers debe ir DESPUÉS de app.UseAuthorization() para que las políticas de autorización se apliquen correctamente

try
{
    app.MapControllers();
}
catch (System.Reflection.ReflectionTypeLoadException ex)
{
    var logger = app.Logger;
    logger.LogError(ex, "ReflectionTypeLoadException al mapear controladores: {Message}", ex.Message);

    var loaderExceptions = ex.LoaderExceptions ?? Array.Empty<Exception>();
    foreach (var le in loaderExceptions)
    {
        logger.LogError(le, "LoaderException: {Type} - {Message}", le.GetType().FullName, le.Message);

        if (le is System.IO.FileNotFoundException fnf)
        {
            logger.LogError("FileNotFoundException.FileName = {FileName}", fnf.FileName);
            if (!string.IsNullOrEmpty(fnf.FusionLog))
            {
                logger.LogError("FusionLog: {FusionLog}", fnf.FusionLog);
            }
        }

        if (le is System.BadImageFormatException biformat)
        {
            logger.LogError("BadImageFormatException: {Message}", biformat.Message);
        }

        // Si hay excepciones internas más informativas, registrarlas también
        if (le.InnerException != null)
        {
            logger.LogError(le.InnerException, "Inner exception: {Message}", le.InnerException.Message);
        }
    }

    // No relanzamos la excepción para evitar que la aplicación termine durante el arranque.
    // Esto permite iniciar la aplicación aunque los controladores no se hayan podido mapear.
    logger.LogError("Mapeo de controladores falló; la aplicación continuará sin endpoints de controllers hasta resolver la causa raíz.");
}

app.Run();