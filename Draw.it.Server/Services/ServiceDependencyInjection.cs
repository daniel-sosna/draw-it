using Draw.it.Server.Services.Game;
using Draw.it.Server.Services.Room;
using Draw.it.Server.Services.User;
using Draw.it.Server.Services.WordPool;

namespace Draw.it.Server.Services;

public static class ServiceDependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<IUserService, UserService>();
        services.AddSingleton<IRoomService, RoomService>();
        services.AddSingleton<IWordPoolService, WordPoolService>();
        services.AddSingleton<IGameService, GameService>();
        return services;
    }
}