using MediatR;
using Student.Command.Domain.Commands;

namespace Student.Command.Application.Features.Students.DeleteStudent
{
    public record DeleteStudentCommand(Guid Id, Guid UserId) : IRequest, IDeleteStudentCommand;
}
