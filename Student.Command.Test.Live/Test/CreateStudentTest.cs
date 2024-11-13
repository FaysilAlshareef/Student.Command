using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Student.Command.Domain.Events;
using Student.Command.Domain.Events.DataTypes;
using Student.Command.Domain.Resourses;
using Student.Command.Grpc;
using Student.Command.Test.Asserts;
using Student.Command.Test.Helpers;
using Student.Command.Test.Live.Asserts;
using Student.Command.Test.Live.Helpers;
using Student.Command.Test.Protos;
using Xunit.Abstractions;

namespace Student.Command.Test.Live.Test
{
    public class CreateStudentTest : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly DbContextHelper _dbContextHelper;
        private readonly GrpcClientHelper _grpcClientHelper;
        private readonly ListenerHelper _listenerHelper;
        public CreateStudentTest(WebApplicationFactory<Program> factory, ITestOutputHelper helper)
        {
            factory = factory.WithDefaultConfigurations(helper, services =>
            {
                services.SetLiveTestsDefaultEnvironment();
            });

            _dbContextHelper = new DbContextHelper(factory.Services);
            _grpcClientHelper = new GrpcClientHelper(factory);
            _listenerHelper = new ListenerHelper(factory.Services);
        }

        [Fact]
        public async Task CreateStudent_SendValidData_ReturnStudentCreated()
        {
            // Arrange
            var createStudentRequest = new CreateStudentRequest
            {
                Name = "Ali",
                Address = "Tripoli",
                Phone = "0923361144",
                UserId = Guid.NewGuid().ToString()
            };

            var listener = _listenerHelper.Listener;

            // Act
            var response = await _grpcClientHelper.Send(r => r.CreateStudentAsync(createStudentRequest));

            await Task.Delay(10000);

            var @event = await _dbContextHelper.Query(db => db.Events.OfType<StudentCreated>().SingleOrDefaultAsync());

            var outboxMessage = await _dbContextHelper.Query(db => db.OutboxMessages.SingleOrDefaultAsync());

            await listener.CloseAsync();

            var message = listener.Messages.FirstOrDefault();

            // Assert
            Assert.Null(outboxMessage);

            Assert.Equal(Phrases.StudentCreated, response.Message);

            createStudentRequest.AssertEquality(@event);

            MessageAssert.AssertEquality<StudentCreated, StudentCreatedData>(@event, message);
        }

    }
}
