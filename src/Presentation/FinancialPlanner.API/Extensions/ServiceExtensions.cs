using FinancialPlanner.API.Extensions;

namespace FinancialPlanner.API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddPresentationLayerServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApiLayerServices();
        services.AddApplicationServices();
        services.AddInfrastructureServices(configuration); 

        return services;
    }
}