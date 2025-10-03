using Microsoft.OpenApi.Models;
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

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
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
