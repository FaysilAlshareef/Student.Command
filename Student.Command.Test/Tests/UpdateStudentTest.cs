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

namespace Student.Command.Test.Tests;

public class UpdateStudentTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly DbContextHelper _dbContextHelper;
    private readonly GrpcClientHelper _grpcClientHelper;

    public UpdateStudentTest(WebApplicationFactory<Program> factory, ITestOutputHelper helper)
    {
        factory = factory.WithDefaultConfigurations(helper, services =>
        {
            services.SetUnitTestsDefaultEnvironment();
        });

        _dbContextHelper = new DbContextHelper(factory.Services);
        _grpcClientHelper = new GrpcClientHelper(factory);
    }



    #region General Methods
    private async Task<(Guid aggregateId, Guid userId, StudentCreated create_student_event)> CreateFakeStudent(
            string name = default!,
            string phone = default!,
            string address = default!)
    {

        bool allNull = new object[] { name, address, phone }.All(x => x == null);
        Guid aggregateId = default!;
        Guid userId = Guid.NewGuid();

        // Arrange - Student Creation
        var createStudentRequest = new CreateStudentRequest()
        {
            Name = "StudentName",
            Address = "Tripoli",
            Phone = "0923361144",
            UserId = userId.ToString()
        };

        if (!allNull)
            createStudentRequest = new CreateStudentRequest()
            {
                Address = address,
                Name = name,
                Phone = phone,
                UserId = userId.ToString(),
            };

        // Act - Student Creation
        var create_student_response = await _grpcClientHelper.Send(r => r.CreateStudentAsync(createStudentRequest));
        var create_student_event = await _dbContextHelper.Query(db => db.Events.OfType<StudentCreated>().SingleOrDefaultAsync());

        // Assert - Student Creation
        Assert.NotNull(create_student_event);
        Assert.Equal(Phrases.StudentCreated, create_student_response.Message);

        aggregateId = create_student_event.AggregateId;

        return (aggregateId, userId, create_student_event);
    }

    private void AssertsVerification(Guid aggregateId, StudentUpdated? update_student_event, UpdateStudentRequest updateStudentRequest, string responseMessage)
    {
        // Assert - Verify updating response
        Assert.NotNull(update_student_event);
        Assert.Equal(Phrases.StudentUpdated, responseMessage);

        // Verify Event Integrity
        Assert.Equal(aggregateId.ToString(), update_student_event.AggregateId.ToString());
        Assert.Equal(updateStudentRequest.UserId, update_student_event.UserId.ToString());
        Assert.Equal(updateStudentRequest.Id, update_student_event.AggregateId.ToString());
        Assert.Equal(updateStudentRequest.Name, update_student_event.Data.Name);
        Assert.Equal(updateStudentRequest.Address, update_student_event.Data.Address);
        Assert.Equal(updateStudentRequest.Phone, update_student_event.Data.Phone);
        Assert.Equal(EventType.StudentUpdated, update_student_event.Type);
    }

    #endregion


    [Fact]
    public async Task UpdateStudent_SendValidData_VerifyDataChanges()
    {
        var (aggregateId, userId, create_student_event) = await CreateFakeStudent();

        // Arrange
        var updateStudentRequest = new UpdateStudentRequest
        {
            Id = aggregateId.ToString(),
            Name = "Ali Ahmed",
            Address = "Tripoli, Libya",
            Phone = "0913361145",
            UserId = userId.ToString()
        };

        // Act 
        var update_student_response = await _grpcClientHelper.Send(r => r.UpdateStudentAsync(updateStudentRequest));
        var update_student_event = await _dbContextHelper.Query(db => db.Events.OfType<StudentUpdated>().SingleOrDefaultAsync());
        await _dbContextHelper.UpdateAsync(update_student_event);

        // Assert
        AssertsVerification(aggregateId, update_student_event, updateStudentRequest, update_student_response.Message);
        Assert.Equal((create_student_event.Sequence + 1).ToString(), update_student_event.Sequence.ToString());
    }

    [Fact]
    public async Task UpdateStudent_MultipleUpdates_SendValidData_VerifySequenceIncrementAndDataChanges()
    {
        var (aggregateId, userId, create_student_event) = await CreateFakeStudent();
        int expected_sequence = create_student_event.Sequence;

        var updateStudentRequests = new List<UpdateStudentRequest>();

        // Arrange - Student New Data
        for (int i = 1; i < 10; i++)
        {
            var student_fake_data = new StudentUpdatedDataFaker().Generate();
            updateStudentRequests.Add(new UpdateStudentRequest
            {
                Id = aggregateId.ToString(),
                UserId = userId.ToString(),
                Name = student_fake_data.Name,
                Address = student_fake_data.Address,
                Phone = student_fake_data.Phone,
            });
        }

        foreach (var updateStudentRequest in updateStudentRequests)
        {
            expected_sequence++;

            // Act - Update Student
            var update_student_response = await _grpcClientHelper.Send(r => r.UpdateStudentAsync(updateStudentRequest));
            var update_student_event = await _dbContextHelper.Query(db => db.Events.OfType<StudentUpdated>().OrderByDescending(s => s.Sequence).FirstOrDefaultAsync());

            // Assert
            AssertsVerification(aggregateId, update_student_event, updateStudentRequest, update_student_response.Message);

            // Verify Sequence Increment
            Assert.Equal(expected_sequence, update_student_event.Sequence);

            await _dbContextHelper.UpdateAsync(update_student_event);
        }
    }

    [Fact]
    public async Task UpdateStudent_SendSameData_ThrowUpdateStudentFailed()
    {
        var (aggregateId, userId, create_student_event) = await CreateFakeStudent();

        // Arrange - Same Creation Student Data
        var updateStudentRequest = new UpdateStudentRequest
        {
            Id = aggregateId.ToString(),
            Name = create_student_event.Data.Name,
            Address = create_student_event.Data.Address,
            Phone = create_student_event.Data.Phone,
            UserId = userId.ToString()
        };

        // Act 
        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await _grpcClientHelper.Send(
                x => x.UpdateStudentAsync(updateStudentRequest)));

        var update_student_event = await _dbContextHelper.Query(
            db => db.Events.OfType<StudentUpdated>().SingleOrDefaultAsync());

        // Assert - Verify Updating Failed
        Assert.NotEmpty(exception.Status.Detail);
        Assert.Equal(StatusCode.FailedPrecondition, exception.StatusCode);
        Assert.Equal(Phrases.UpdateStudentFaild, exception.Status.Detail);

        Assert.Null(update_student_event);
    }

    [Fact]
    public async Task UpdateStudent_SendValidDateWithInvalidUserId_ThrowStudentNotFound()
    {
        // Arrange - Generate Fake Student Data
        var student_fake_data = new StudentUpdatedFaker().Generate();
        var updateStudentRequest = new UpdateStudentRequest
        {
            Id = student_fake_data.AggregateId.ToString(),
            Name = student_fake_data.Data.Name,
            Address = student_fake_data.Data.Address,
            Phone = student_fake_data.Data.Phone,
            UserId = student_fake_data.UserId.ToString()
        };

        // Act 
        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await _grpcClientHelper.Send(
                x => x.UpdateStudentAsync(updateStudentRequest)));

        var update_student_event = await _dbContextHelper.Query(
            db => db.Events.OfType<StudentUpdated>().SingleOrDefaultAsync());

        // Assert - Verify Student Not Found
        Assert.NotEmpty(exception.Status.Detail);
        Assert.Equal(StatusCode.NotFound, exception.StatusCode);
        Assert.Equal(Phrases.StudentNotFound, exception.Status.Detail);

        Assert.Null(update_student_event);
    }

    [Theory]
    [InlineData("A", "0925553311", "test address", "Name")]
    [InlineData("Ali", "555", "test address", "Phone")]
    [InlineData("Ali", "566666666666666666655", "test address", "Phone")]
    [InlineData("Ali", "0925553311", "t", "Address")]
    public async Task UpdateStudent_SendInvalidData_ThrowInvalidData(string name, string phone, string address, string error)
    {

        // Arrange
        var (aggregateId, userId, create_student_event) = await CreateFakeStudent();
        var updateStudentRequest = new UpdateStudentRequest
        {
            Id = aggregateId.ToString(),
            Name = name,
            Address = address,
            Phone = phone,
            UserId = userId.ToString()
        };

        // Act
        var exception = await Assert.ThrowsAsync<RpcException>(
           async () => await _grpcClientHelper.Send(x => x.UpdateStudentAsync(updateStudentRequest)));

        var update_student_event = await _dbContextHelper.Query(
            db => db.Events.OfType<StudentUpdated>().OrderByDescending(s => s.Sequence).LastOrDefaultAsync());

        //Assert
        Assert.Null(update_student_event);

        Assert.NotEmpty(exception.Status.Detail);
        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);

        Assert.Contains(exception.GetValidationErrors(), e => e.PropertyName.EndsWith(error));
    }
}
