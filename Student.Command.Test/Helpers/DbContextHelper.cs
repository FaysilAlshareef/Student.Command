using Microsoft.Extensions.DependencyInjection;
using Student.Command.Domain.Entities;
using Student.Command.Infra.Persistence;

namespace Student.Command.Test.Helpers
{
    public class DbContextHelper(IServiceProvider provider)
    {
        private readonly IServiceProvider _provider = provider;

        public async Task<TResult> Query<TResult>(Func<AppDbCon, Task<TResult>> query)
        {
            using var scope = _provider.CreateScope();
            var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbCon>();
            return await query(appDbContext);
        }

        public async Task<Event> InsertAsync(Event @event)
        {
            using var scope = _provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbCon>();
            await context.Events.AddAsync(@event);
            await context.SaveChangesAsync();
            return @event;
        }

        public async Task<Event> UpdateAsync(Event @event)
        {
            using var scope = _provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbCon>();
            context.Events.Update(@event);
            await context.SaveChangesAsync();
            return @event;
        }
    }
}
