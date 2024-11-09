using Student.Command.Domain.Entities;
using Student.Command.Domain.Events.DataTypes;

namespace Student.Command.Domain.Events
{
    public class StudentDeleted(
        Guid aggregateId,
        Guid userId,
        int sequence,
        StudentDeletedData data,
        int version = 1) : Event<StudentDeletedData>(aggregateId, userId, sequence, data, version)
    {
    }
}
