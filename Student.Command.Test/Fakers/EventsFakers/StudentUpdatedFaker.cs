using Bogus.Extensions;
using Student.Command.Domain.Events;
using Student.Command.Domain.Events.DataTypes;

namespace Student.Command.Test.Fakers.EventsFakers;

public class StudentUpdatedFaker : CustomConstructorFaker<StudentUpdated>
{
    public StudentUpdatedFaker()
    {
        RuleFor(r => r.AggregateId, f => f.Random.Guid());
        RuleFor(r => r.Sequence, 1);
        RuleFor(r => r.UserId, f => f.Random.Guid());
        RuleFor(r => r.Version, 1);
        RuleFor(r => r.DateTime, DateTime.UtcNow);
        RuleFor(r => r.Data, f => new StudentUpdatedDataFaker());
    }
}

public class StudentUpdatedDataFaker : CustomConstructorFaker<StudentUpdatedData>
{
    public StudentUpdatedDataFaker()
    {
        RuleFor(r => r.Name, f => f.Name.FullName());
        RuleFor(r => r.Address, f => f.Address.FullAddress().ClampLength(3, 40));
        RuleFor(r => r.Phone, f => f.Phone.PhoneNumber("09########"));
    }
}
