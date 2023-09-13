using FluentValidation;
using simpliBuild.SWMS.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SimpliBuild.SWMS.Validation
{
    public class ProjectValidator : AbstractValidator<SimpliProject>
    {
        public ProjectValidator()
        {
            RuleFor(project => project.Address1).NotEmpty().WithMessage("Street is required.");
            RuleFor(project => project.Suburb).NotEmpty().WithMessage("Suburb is required.");
            RuleFor(project => project.PostCode).NotEmpty().WithMessage("Post Code is required.");
            RuleFor(project => project.Name).NotEmpty().WithMessage("Project Name is required.");
        }
    }
}
