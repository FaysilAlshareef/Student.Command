using FluentValidation;
using Student.Command.Grpc.Protos;

namespace Student.Command.Grpc.Validators
{
    public class DeleteStudentRequestValidator : AbstractValidator<DeleteStudentRequest>
    {
        public DeleteStudentRequestValidator()
        {
            RuleFor(r => r.Id)
                .Must(id => Guid.TryParse(id, out var _))
                .NotEqual(Guid.Empty.ToString());
        }
    }
}
