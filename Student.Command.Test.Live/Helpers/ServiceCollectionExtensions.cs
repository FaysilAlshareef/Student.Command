using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Student.Command.Infra.Persistence;
using Student.Command.Test.Helpers;

namespace Student.Command.Test.Live.Helpers
{
    public static class ServiceCollectionExtensions
    {
        public static void SetLiveTestsDefaultEnvironment(this IServiceCollection services)
        {
            UseSqlDatabaseTesting(services);
        }

        private static void UseSqlDatabaseTesting(IServiceCollection services)
        {
            var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();

            services.Configure<SqlDbOptions>(c => c.SetLiveTestOptions(configuration));

            var descriptor = services.Single(d => d.ServiceType == typeof(DbContextOptions<AppDbCon>));

            services.Remove(descriptor);

            services.AddDbContext<AppDbCon>((provider, configure) =>
            {
                var options = provider.GetRequiredService<IOptions<SqlDbOptions>>();

                configure.UseSqlServer(options.Value.Database);
            });

            services.AddHostedService<DbTruncate>();
        }
    }
}
