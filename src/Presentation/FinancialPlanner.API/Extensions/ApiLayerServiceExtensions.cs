using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text.Json.Serialization;

namespace FinancialPlanner.API.Extensions;

public static class ApiLayerServiceExtensions
{
    public static IServiceCollection AddApiLayerServices(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(name: "AllowSpecificOrigins",
                              policy =>
                              {
                                  policy.WithOrigins("http://localhost:4200")
                                        .AllowAnyHeader()
                                        .AllowAnyMethod();
                              });
        });

        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        });

        // This is the crucial line that was missing. It registers the service
        // that allows the DbContext to access the current HttpContext.
        services.AddHttpContextAccessor();

        services.AddEndpointsApiExplorer();

        var informationalVersion = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Financial Planner API",
                Version = informationalVersion,
                Description = "API for managing personal finances."
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter a valid token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer"
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

        return services;
    }
}

