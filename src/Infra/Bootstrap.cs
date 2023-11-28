using Infra.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infra;

public static class Bootstrap
{
    public static void AddInfra(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<MasterOfScissorContext>(options =>
        {
            options.UseMySql(configuration.GetConnectionString("DefaultConnection"),
                new MySqlServerVersion(new Version(8, 0, 26)),
                b => b.MigrationsAssembly("Infra"));
        });
    }

    public static void RunMigrations(IServiceProvider service)
    {
        //Run EF Core Migrations
        using var scope = service.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MasterOfScissorContext>();
        context.Database.Migrate();

    }
}
