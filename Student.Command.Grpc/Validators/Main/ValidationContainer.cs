using Calzolari.Grpc.AspNetCore.Validation;

namespace Student.Command.Grpc.Validators.Main;

public static class ValidationContainer
{
    public static IServiceCollection AddAppValidators(this IServiceCollection services)
    {
        services.AddGrpcValidation();

        services.AddScoped<GrpcValidator>();

        services.AddValidator<CreateStudentRequestValidator>();

        services.AddValidator<UpdateStudentRequestValidator>();

        services.AddValidator<DeleteStudentRequestValidator>();

        return services;
    }
}
