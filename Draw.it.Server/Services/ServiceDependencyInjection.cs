using Draw.it.Server.Services.Game;
using Draw.it.Server.Services.Room;
using Draw.it.Server.Services.User;
using Draw.it.Server.Services.WordPool;

namespace Draw.it.Server.Services;

public static class ServiceDependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoomService, RoomService>();
        services.AddScoped<IWordPoolService, WordPoolService>();
        services.AddScoped<IGameService, GameService>();
        return services;
    }
}