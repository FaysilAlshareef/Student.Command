using Calzolari.Grpc.Net.Client.Validation;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Student.Command.Domain.Events;
using Student.Command.Domain.Resourses;
using Student.Command.Grpc;
using Student.Command.Test.Asserts;
using Student.Command.Test.Helpers;
using Student.Command.Test.Protos;
using Xunit.Abstractions;

namespace Student.Command.Test.Tests
{
    public class CreateStudentTest : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly DbContextHelper _dbContextHelper;
        private readonly GrpcClientHelper _grpcClientHelper;
        public CreateStudentTest(WebApplicationFactory<Program> factory, ITestOutputHelper helper)
        {
            factory = factory.WithDefaultConfigurations(helper, services =>
            {
                services.SetUnitTestsDefaultEnvironment();
            });

            _dbContextHelper = new DbContextHelper(factory.Services);
            _grpcClientHelper = new GrpcClientHelper(factory);
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

            // Act
            var response = await _grpcClientHelper.Send(r => r.CreateStudentAsync(createStudentRequest));

            var @event = await _dbContextHelper.Query(db => db.Events.OfType<StudentCreated>().SingleOrDefaultAsync());

            var outboxMessage = await _dbContextHelper.Query(db => db.OutboxMessages.Include(o => o.Event).SingleOrDefaultAsync());

            // Assert
            Assert.NotNull(@event);

            Assert.NotNull(outboxMessage);

            Assert.Equal(Phrases.StudentCreated, response.Message);

            createStudentRequest.AssertEquality(@event);

            Assert.Equal(@event.Id, outboxMessage.Event!.Id);
        }

        [Theory]
        [InlineData("A", "0925553311", "test address", "Name")]
        [InlineData("Ali", "555", "test address", "Phone")]
        [InlineData("Ali", "566666666666666666655", "test address", "Phone")]
        [InlineData("Ali", "0925553311", "t", "Address")]
        public async Task CreateStudent_SendInvalidData_ThrowInvalidArguemt(
            string name,
            string phone,
            string address,
            string error
            )
        {
            // Arrange

            var createStudentRequest = new CreateStudentRequest
            {
                Name = name,
                Address = address,
                Phone = phone,
                UserId = Guid.NewGuid().ToString()
            };

            // Act
            var exception = await Assert.ThrowsAsync<RpcException>(
               async () => await _grpcClientHelper.Send(x => x.CreateStudentAsync(createStudentRequest)));


            var @event = await _dbContextHelper.Query(db => db.Events.SingleOrDefaultAsync());

            //Assert

            Assert.NotEmpty(exception.Status.Detail);

            Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);

            Assert.Contains(
                   exception.GetValidationErrors(),
                   e => e.PropertyName.EndsWith(error)
                          );

            Assert.Null(@event);

        }
    }
}
