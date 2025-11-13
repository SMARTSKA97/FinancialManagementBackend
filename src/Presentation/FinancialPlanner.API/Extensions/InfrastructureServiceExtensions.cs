using FinancialPlanner.Application;
using FinancialPlanner.Application.Contracts;
using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Infrastructure.Persistence;
using FinancialPlanner.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace FinancialPlanner.API.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Configure strongly typed settings objects
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        // 2. Register the DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
            o => o.MigrationsHistoryTable("__EFMigrationsHistory", "identity")));

        // 3. Register Identity
        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ ";
            // Relax password requirements for easier development
            options.Password.RequireDigit = false;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>();

        // 4. Register JWT Authentication
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()!;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
            };
        });

        // --- THIS IS THE FIX ---
        // 5. Register Repositories and the Unit of Work
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}