using Student.Command.Domain.Commands;
using Student.Command.Domain.Events;
using Student.Command.Domain.Exceptions;
using Student.Command.Domain.Exceptions.Abstraction;
using Student.Command.Domain.Exceptions.Abstraction.Exceptions;
using Student.Command.Domain.Extensions;
using Student.Command.Domain.Resourses;

namespace Student.Command.Domain.Models
{
    public class Student : Aggregate<Student>
    {
        private Student() { }
        public string Name { get; private set; } = string.Empty;
        public string Phone { get; private set; } = string.Empty;
        public string Address { get; private set; } = string.Empty;
        public bool Deleted { get; private set; }

        public static Student Create(ICreateStudentCommand command)
        {
            var student = new Student();

            var @event = command.ToEvent();

            student.ApplyChange(@event);

            return student;
        }

        public void Apply(StudentCreated @event)
        {
            Name = @event.Data.Name;
            Phone = @event.Data.Phone;
            Address = @event.Data.Address;
        }

        public void Update(IUpdateStudentCommand command)
        {
            if (Name == command.Name && Phone == command.Phone && Address == command.Address)
                throw new StudentUpdateFaildException();

            var @event = command.ToEvent(Sequence + 1);

            ApplyChange(@event);
        }

        public void Apply(StudentUpdated @event)
        {
            Name = @event.Data.Name;
            Phone = @event.Data.Phone;
            Address = @event.Data.Address;
        }

        public void Delete(IDeleteStudentCommand command)
        {
            if (Deleted)
                throw new AppException(ExceptionStatusCode.FailedPrecondition, Phrases.StudentAlreadyDeleted);

            var @event = command.ToEvent(Sequence + 1);

            ApplyChange(@event);
        }

        public void Apply(StudentDeleted _)
        {
            Deleted = true;
        }
    }
}
