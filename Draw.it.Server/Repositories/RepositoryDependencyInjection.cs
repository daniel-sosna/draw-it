using Draw.it.Server.Enums;
using Draw.it.Server.Repositories.Room;
using Draw.it.Server.Repositories.Session;
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
            services.AddSingleton<ISessionRepository, InMemSessionRepository>();
        }
        else
        {
            // add dependencies with db implementation here
        }

        return services;
    }
}