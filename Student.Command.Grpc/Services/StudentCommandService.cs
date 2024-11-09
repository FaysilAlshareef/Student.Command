using Grpc.Core;
using MediatR;
using Student.Command.Domain.Resourses;
using Student.Command.Grpc.Extensions;
using Student.Command.Grpc.Protos;

namespace Student.Command.Grpc.Services
{
    public class StudentCommandService(IMediator mediator) : StudentCommand.StudentCommandBase
    {
        private readonly IMediator _mediator = mediator;

        public override async Task<Response> CreateStudent(CreateStudentRequest request, ServerCallContext context)
        {
            var command = request.ToCommand();

            await _mediator.Send(command);

            return new Response
            {
                Message = Phrases.StudentCreated
            };
        }

        public override async Task<Response> UpdateStudent(UpdateStudentRequest request, ServerCallContext context)
        {
            var command = request.ToCommand();

            await _mediator.Send(command);

            return new Response
            {
                Message = Phrases.StudentUpdated
            };
        }

        public override async Task<Response> DeleteStudent(DeleteStudentRequest request, ServerCallContext context)
        {
            var command = request.ToCommand();

            await _mediator.Send(command);

            return new Response
            {
                Message = Phrases.StudentDeleted
            };
        }
    }
}
