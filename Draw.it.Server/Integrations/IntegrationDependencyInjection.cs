using Draw.it.Server.Integrations.Gemini;

namespace Draw.it.Server.Integrations;

public static class IntegrationDependencyInjection
{
    public static IServiceCollection AddApplicationIntegrations(this IServiceCollection services)
    {
        services.AddScoped<IGeminiClient, GeminiClient>();
        return services;
    }
}