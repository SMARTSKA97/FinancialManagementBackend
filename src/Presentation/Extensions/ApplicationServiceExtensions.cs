using Application.Contracts;
using FluentValidation;
using System.Reflection;
using Application.DTOs.AccountCategory;
using Application.DTOs.Categories;
using Application.DTOs.TransactionCategory;
using Application.Services;

namespace API.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()));

        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddScoped<ICategoryService<AccountCategoryDto, UpsertAccountCategoryDto>, AccountCategoryService>();
        services.AddScoped<ICategoryService<TransactionCategoryDto, UpsertTransactionCategoryDto>, TransactionCategoryService>();

        services.AddScoped<IDashboardService, DashboardService>();

        services.AddScoped<IssueRankingService>();
        services.AddScoped<IssueSimilarityService>();
        services.AddScoped<TaxonomySeederService>();

        return services;
    }
}