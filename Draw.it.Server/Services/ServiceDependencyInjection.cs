using Draw.it.Server.Services.Rooms;
using Draw.it.Server.Services.User;

namespace Draw.it.Server.Services;

public static class ServiceDependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<IUserService, UserService>();
        services.AddSingleton<IRoomService, RoomService>();
        return services;
    }
}