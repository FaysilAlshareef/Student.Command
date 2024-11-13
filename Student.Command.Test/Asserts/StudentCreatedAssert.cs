using Student.Command.Domain.Enums;
using Student.Command.Domain.Events;
using Student.Command.Test.Protos;

namespace Student.Command.Test.Asserts
{
    public static class StudentCreatedAssert
    {
        public static void AssertEquality(this CreateStudentRequest request, StudentCreated? studentCreated)
        {
            Assert.Equal(request.UserId, studentCreated.UserId.ToString());
            Assert.Equal(request.Name, studentCreated.Data.Name);
            Assert.Equal(request.Address, studentCreated.Data.Address);

            Assert.Equal(request.Phone, studentCreated.Data.Phone);

            Assert.Equal(DateTime.UtcNow, studentCreated.DateTime, TimeSpan.FromMinutes(1));

            Assert.Equal(EventType.StudentCreated, studentCreated.Type);
        }
    }
}
