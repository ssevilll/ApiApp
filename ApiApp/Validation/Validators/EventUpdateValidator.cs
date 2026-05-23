using ApiApp.DTOs.EventDtos;
using FluentValidation;

namespace ApiApp.Validation.Validators
{
    public class EventUpdateValidator : AbstractValidator<EventUpdateDto>
    {
        public EventUpdateValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(150).WithMessage("Title must not exceed 150 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description must not exceed 500 characters.")
                .When(x => x.Description != null);

            RuleFor(x => x.Date)
                .NotEmpty().WithMessage("Date is required.")
                .GreaterThan(DateTime.Now).WithMessage("Event date must be a future date.");

            RuleFor(x => x.Location)
                .NotEmpty().WithMessage("Location is required.")
                .MaximumLength(200).WithMessage("Location must not exceed 200 characters.");

            RuleFor(x => x.OrganizerId)
                .GreaterThan(0).WithMessage("A valid OrganizerId is required.");
        }
    }
}
