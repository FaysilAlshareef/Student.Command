namespace Student.Command.Domain.Commands
{
    public interface IDeleteStudentCommand
    {
        Guid Id { get; }
        Guid UserId { get; }
    }
}
