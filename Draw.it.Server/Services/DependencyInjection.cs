using Draw.it.Server.Services.User;

namespace Draw.it.Server.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IUserService, UserService>();
        return services;
    }
}