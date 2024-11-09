using Student.Command.Domain.Enums;

namespace Student.Command.Domain.Events.DataTypes
{
    public record StudentDeletedData : IEventData
    {
        public EventType Type => EventType.StudentDeleted;
    }
}
