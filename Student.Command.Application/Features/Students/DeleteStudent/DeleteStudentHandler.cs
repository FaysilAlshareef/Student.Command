using MediatR;
using Student.Command.Application.Contracts.Repositories;
using Student.Command.Application.Contracts.Services;
using Student.Command.Domain.Exceptions;

namespace Student.Command.Application.Features.Students.DeleteStudent
{
    public class DeleteStudentHandler : IRequestHandler<DeleteStudentCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommitEventsService _commitEventsService;

        public DeleteStudentHandler(IUnitOfWork unitOfWork, ICommitEventsService commitEventsService)
        {
            _unitOfWork = unitOfWork;
            _commitEventsService = commitEventsService;
        }
        public async Task Handle(DeleteStudentCommand command, CancellationToken cancellationToken)
        {
            var events = await _unitOfWork.Events.GetAllByAggregateIdAsync(command.Id, cancellationToken);

            if (!events.Any())
                throw new StudentNotFoundException();

            var student = Domain.Models.Student.LoadFromHistory(events);

            student.Delete(command);

            await _commitEventsService.CommitNewEventsAsync(student);
        }
    }
}
