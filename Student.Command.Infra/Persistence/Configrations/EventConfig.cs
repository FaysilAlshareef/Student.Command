using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Student.Command.Domain.Entities;
using Student.Command.Domain.Enums;
using Student.Command.Domain.Events;

namespace Student.Command.Infra.Persistence.Configrations
{
    public class EventConfig : IEntityTypeConfiguration<Event>
    {
        public void Configure(EntityTypeBuilder<Event> builder)
        {
            builder.HasIndex(e => new { e.AggregateId, e.Sequence }).IsUnique();

            builder.Property(e => e.Type)
                .HasMaxLength(128)
                .HasConversion<string>();

            builder.HasDiscriminator(e => e.Type)
                .HasValue<StudentCreated>(EventType.StudentCreated)
                .HasValue<StudentUpdated>(EventType.StudentUpdated)
                .HasValue<StudentDeleted>(EventType.StudentDeleted);
        }
    }
}
