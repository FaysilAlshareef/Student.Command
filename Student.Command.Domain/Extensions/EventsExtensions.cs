using Student.Command.Domain.Commands;
using Student.Command.Domain.Events;

namespace Student.Command.Domain.Extensions
{
    public static class EventsExtensions
    {
        public static StudentCreated ToEvent(this ICreateStudentCommand command)
            => new(
                Guid.NewGuid(),
                command.UserId,
                new Events.DataTypes.StudentCreatedData(
                    command.Name,
                    command.Phone,
                    command.Address));

        public static StudentUpdated ToEvent(this IUpdateStudentCommand command, int sequence)
            => new(
                command.Id,
                command.UserId,
                sequence,
                new Events.DataTypes.StudentUpdatedData(
                    command.Name,
                    command.Phone,
                    command.Address));

        public static StudentDeleted ToEvent(this IDeleteStudentCommand command, int sequence)
           => new(
               command.Id,
               command.UserId,
               sequence,
               new Events.DataTypes.StudentDeletedData());
    }
}
