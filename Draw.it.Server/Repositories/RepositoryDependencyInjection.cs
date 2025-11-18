using Draw.it.Server.Enums;
using Draw.it.Server.Data;
using Draw.it.Server.Repositories.Game;
using Draw.it.Server.Repositories.Room;
using Draw.it.Server.Repositories.WordPool;
using Draw.it.Server.Repositories.User;
using Microsoft.EntityFrameworkCore;

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
            // Register EF Core DbContext if connection string present
            var connectionString = config.GetConnectionString("Postgres")
                ?? throw new InvalidOperationException("Connection string 'Postgres' not found.");

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString));

            // Use DB-backed repositories
            services.AddScoped<IUserRepository, DbUserRepository>();
            services.AddScoped<IRoomRepository, DbRoomRepository>();

            // Keep existing singletons
            services.AddSingleton<IWordPoolRepository, FileStreamWordPoolRepository>();
            services.AddSingleton<IGameRepository, InMemGameRepository>();
        }

        return services;
    }
}