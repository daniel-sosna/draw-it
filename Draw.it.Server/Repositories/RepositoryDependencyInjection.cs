using Draw.it.Server.Enums;
using Draw.it.Server.Repositories.Game;
using Draw.it.Server.Repositories.Room;
using Draw.it.Server.Repositories.WordPool;
using Draw.it.Server.Repositories.User;

namespace Draw.it.Server.Repositories;


public static class RepositoryDependencyInjection
{
    public static IServiceCollection AddApplicationRepositories(this IServiceCollection services, IConfiguration config)
    {
        var repoType = config.GetValue<string>("RepositoryType");

        if (repoType == nameof(RepoType.InMem))
        {
            services.AddSingleton<IUserRepository, InMemUserRepository>();
            services.AddSingleton<IRoomRepository, InMemRoomRepository>();
            services.AddSingleton<IWordPoolRepository, FileStreamWordPoolRepository>();
            services.AddSingleton<IGameRepository, InMemGameRepository>();
        }
        else
        {
            services.AddScoped<IUserRepository, EfUserRepository>();
            services.AddScoped<IRoomRepository, EfRoomRepository>();
            services.AddSingleton<IWordPoolRepository, FileStreamWordPoolRepository>();
        }

        return services;
    }
}