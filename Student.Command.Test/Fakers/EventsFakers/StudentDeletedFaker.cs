using Student.Command.Domain.Events;
using Student.Command.Domain.Events.DataTypes;

namespace Student.Command.Test.Fakers.EventsFakers
{
    public class StudentDeletedFaker : CustomConstructorFaker<StudentDeleted>
    {
        public StudentDeletedFaker(Guid aggregateId, int sequence = 2)
        {
            RuleFor(r => r.AggregateId, f => aggregateId);
            RuleFor(r => r.Sequence, sequence);
            RuleFor(r => r.UserId, f => f.Random.Guid());
            RuleFor(r => r.Version, 1);
            RuleFor(r => r.DateTime, DateTime.UtcNow);
            RuleFor(r => r.Data, f => new StudentDeletedData());
        }
    }
}
