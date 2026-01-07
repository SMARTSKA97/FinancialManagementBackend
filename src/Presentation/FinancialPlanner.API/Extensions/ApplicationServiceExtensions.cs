using FinancialPlanner.Application.Contracts;
using FluentValidation;
using System.Reflection;
using FinancialPlanner.Application.DTOs.AccountCategory;
using FinancialPlanner.Application.DTOs.Categories;
using FinancialPlanner.Application.DTOs.TransactionCategory;
using FinancialPlanner.Application.Services;
using FinancialPlanner.Application.Services;

namespace FinancialPlanner.API.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddScoped<ICategoryService<AccountCategoryDto, UpsertAccountCategoryDto>, AccountCategoryService>();
        services.AddScoped<ICategoryService<TransactionCategoryDto, UpsertTransactionCategoryDto>, TransactionCategoryService>();

        services.AddScoped<IDashboardService, DashboardService>();

        return services;
    }
}