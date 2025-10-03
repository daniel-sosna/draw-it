using Draw.it.Server.Services.Room;
using Draw.it.Server.Services.User;
using Draw.it.Server.Services.Session;

namespace Draw.it.Server.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IUserService, UserService>();
        services.AddSingleton<IRoomService, RoomService>();
        services.AddSingleton<ISessionService, SessionService>();
        return services;
    }
}