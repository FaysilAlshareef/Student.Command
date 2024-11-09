using Calzolari.Grpc.Net.Client.Validation;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Student.Command.Domain.Enums;
using Student.Command.Domain.Events;
using Student.Command.Domain.Resourses;
using Student.Command.Grpc;
using Student.Command.Test.Fakers.EventsFakers;
using Student.Command.Test.Helpers;
using Student.Command.Test.Protos;
using Xunit.Abstractions;

namespace Student.Command.Test.Tests
{
    public class DeleteStudentTest : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly DbContextHelper _dbContextHelper;
        private readonly GrpcClientHelper _grpcClientHelper;
        public DeleteStudentTest(WebApplicationFactory<Program> factory, ITestOutputHelper helper)
        {
            factory = factory.WithDefaultConfigurations(helper, services =>
            {
                services.SetUnitTestsDefaultEnvironment();
            });

            _dbContextHelper = new DbContextHelper(factory.Services);
            _grpcClientHelper = new GrpcClientHelper(factory);
        }

        [Fact]
        public async Task DeleteStudent_SendExistStudent_ReturnStudentDeleted()
        {
            // Arrange
            var studentCreated = await _dbContextHelper.InsertAsync(new StudentCreatedFaker().Generate());

            var request = new DeleteStudentRequest
            {
                Id = studentCreated.AggregateId.ToString(),
                UserId = Guid.NewGuid().ToString(),
            };

            // Act

            var resposne = await _grpcClientHelper.Send(r => r.DeleteStudentAsync(request));

            var @event = await _dbContextHelper.Query(db => db.Events.OfType<StudentDeleted>().SingleOrDefaultAsync());

            var outboxMessage = await _dbContextHelper.Query(db => db.OutboxMessages.Include(m => m.Event).SingleOrDefaultAsync());

            // Asseet

            Assert.NotNull(@event);

            Assert.NotNull(outboxMessage);

            Assert.Equal(Phrases.StudentDeleted, resposne.Message);

            Assert.Equal(request.UserId, @event.UserId.ToString());

            Assert.Equal(studentCreated.AggregateId, @event.AggregateId);

            Assert.Equal(DateTime.UtcNow, @event.DateTime, TimeSpan.FromMinutes(1));

            Assert.Equal(EventType.StudentDeleted, @event.Type);

            Assert.Equal(@event.Id, outboxMessage.Event!.Id);
        }

        [Fact]

        public async Task DeleteStudent_SendDeletedStudent_ThrowStudentAlreadyDeleted()
        {
            // Arrange
            var studentCreated = await _dbContextHelper.InsertAsync(new StudentCreatedFaker().Generate());

            var studnetDeleted = await _dbContextHelper.InsertAsync(new StudentDeletedFaker(studentCreated.AggregateId).Generate());

            var request = new DeleteStudentRequest
            {
                Id = studentCreated.AggregateId.ToString(),
                UserId = Guid.NewGuid().ToString(),
            };

            // Act

            var exception = await Assert.ThrowsAsync<RpcException>(
             async () => await _grpcClientHelper.Send(x => x.DeleteStudentAsync(request)));

            var @event = await _dbContextHelper.Query(db => db.Events.SingleOrDefaultAsync(s => s.Sequence == 3));

            // Assert
            Assert.Equal(Phrases.StudentAlreadyDeleted, exception.Status.Detail);

            Assert.Equal(StatusCode.FailedPrecondition, exception.StatusCode);

            Assert.Null(@event);
        }

        [Fact]
        public async Task DeleteStudent_SendNotExistStudent_ThrowStudentNotFound()
        {
            // arrange
            var request = new DeleteStudentRequest
            {
                Id = Guid.NewGuid().ToString(),
                UserId = Guid.NewGuid().ToString(),
            };

            // Act
            var exception = await Assert.ThrowsAsync<RpcException>(
                 async () => await _grpcClientHelper.Send(x => x.DeleteStudentAsync(request)));

            var @event = await _dbContextHelper.Query(db => db.Events.SingleOrDefaultAsync());

            // Assert
            Assert.Equal(Phrases.StudentNotFound, exception.Status.Detail);

            Assert.Equal(StatusCode.NotFound, exception.StatusCode);

            Assert.Null(@event);

        }

        [Fact]
        public async Task DeleteStudent_SendInvalidRequestData_ThrowInvalidArgument()
        {
            // Arrange
            var request = new DeleteStudentRequest
            {
                Id = "55"
            };
            var exception = await Assert.ThrowsAsync<RpcException>(
              async () => await _grpcClientHelper.Send(x => x.DeleteStudentAsync(request)));


            var @event = await _dbContextHelper.Query(db => db.Events.SingleOrDefaultAsync());

            //Assert

            Assert.NotEmpty(exception.Status.Detail);

            Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);

            Assert.Contains(
                   exception.GetValidationErrors(),
                   e => e.PropertyName.EndsWith("Id")
                          );

            Assert.Null(@event);
        }
    }
}
