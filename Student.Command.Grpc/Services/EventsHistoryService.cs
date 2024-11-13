using Grpc.Core;
using Student.Command.Application.Contracts.Repositories;
using Student.Command.Grpc.Extensions;
using Student.Command.Grpc.Protos.EventsHistory;

namespace Student.Command.Grpc.Services
{
    public class EventsHistoryService(IUnitOfWork unitOfWork) : StudentEventsHistory.StudentEventsHistoryBase
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async override Task<Response> GetEvents(GetEventsRequest request, ServerCallContext context)
        {
            var events = await _unitOfWork.Events.GetAsPaginationAsync(request.CurrentPage, request.PageSize);

            var response = new Response();

            response.Events.ToOutputEvent(events);

            return response;
        }
    }
}
