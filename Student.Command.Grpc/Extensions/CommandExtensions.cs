using Student.Command.Application.Features.Students.CreateStudent;
using Student.Command.Application.Features.Students.DeleteStudent;
using Student.Command.Application.Features.Students.UpdateStudent;
using Student.Command.Grpc.Protos;

namespace Student.Command.Grpc.Extensions
{
    public static class CommandExtensions
    {
        public static CreateStudentCommand ToCommand(this CreateStudentRequest request)
            => new(
                request.Name,
                request.Phone,
                request.Address,
                Guid.Parse(request.UserId));

        public static UpdateStudentCommand ToCommand(this UpdateStudentRequest request)
            => new(
                Guid.Parse(request.Id),
                request.Name,
                request.Phone,
                request.Address,
                Guid.Parse(request.UserId));

        public static DeleteStudentCommand ToCommand(this DeleteStudentRequest request)
            => new(Guid.Parse(request.Id), Guid.Parse(request.UserId));
    }
}
