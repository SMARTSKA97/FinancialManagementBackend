using FinancialPlanner.Application;
using FinancialPlanner.Application.Contracts;
using FinancialPlanner.Application.DTOs.AccountCategory;
using FinancialPlanner.Application.DTOs.Categories;
using FinancialPlanner.Application.DTOs.TransactionCategory;
using FinancialPlanner.Application.Services;
using FinancialPlanner.Infrastructure.Repositories;

namespace FinancialPlanner.API.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationLayerServices(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile));

        // --- REGISTER REPOSITORIES ---
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();

        // --- REGISTER SERVICES ---
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<ICategoryService<AccountCategoryDto, UpsertAccountCategoryDto>, AccountCategoryService>();
        services.AddScoped<ICategoryService<TransactionCategoryDto, UpsertTransactionCategoryDto>, TransactionCategoryService>();

        return services;
    }
}
