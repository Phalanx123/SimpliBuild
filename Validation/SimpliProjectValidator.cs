using FluentValidation;
using simpliBuild.SWMS.Model;

namespace simpliBuild.Validation;

public class SimpliProjectValidator : AbstractValidator<SimpliProject>
    {
    public SimpliProjectValidator()
    {
        // Validation rules here
        RuleFor(project => project.Name).NotEmpty().WithMessage("Project name is required.");
        RuleFor(project => project.Address1).NotEmpty().WithMessage("Project address is required.");
        RuleFor(project => project.Suburb).NotEmpty().WithMessage("Project suburb is required.");
        RuleFor(project => project.PostCode).NotEmpty().WithMessage("Project postcode is required.");
RuleFor(project => project.State).NotEmpty().WithMessage("Project state is required.");
RuleFor(project => project.Country).NotEmpty().WithMessage("Project country is required.");
    }
}
