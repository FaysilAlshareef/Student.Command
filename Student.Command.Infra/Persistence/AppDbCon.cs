using Microsoft.EntityFrameworkCore;
using Student.Command.Domain.Entities;
using Student.Command.Domain.Events;
using Student.Command.Domain.Events.DataTypes;
using Student.Command.Infra.Persistence.Configrations;

namespace Student.Command.Infra.Persistence
{
    public class AppDbCon(DbContextOptions options) : DbContext(options)
    {
        public DbSet<OutboxMessage> OutboxMessages { get; set; }
        public DbSet<Event> Events { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new EventConfig());

            modelBuilder.ApplyConfiguration(new GenericEventConfig<StudentCreated, StudentCreatedData>());

            modelBuilder.ApplyConfiguration(new GenericEventConfig<StudentUpdated, StudentUpdatedData>());

            modelBuilder.ApplyConfiguration(new GenericEventConfig<StudentDeleted, StudentDeletedData>());

            base.OnModelCreating(modelBuilder);
        }
    }
}
