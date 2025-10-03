namespace FinancialPlanner.API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddApplicationLayerServices();
        return services;
    }

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructureLayerServices(configuration);
        return services;
    }

    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddApiLayerServices();
        return services;
    }
}