using FluentValidation;

namespace ApiApp.DTOs.OrganizerDtos
{
    public class OrganizerUpdateDto
    {
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
    }

    public class OrganizerUpdateDtoValidator : AbstractValidator<OrganizerUpdateDto>
    {
        public OrganizerUpdateDtoValidator()
        {
            RuleFor(o => o.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");
            RuleFor(o => o.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.")
                .MaximumLength(100).WithMessage("Email cannot exceed 100 characters.");
            RuleFor(o => o.Phone)
                .MaximumLength(20).WithMessage("Phone cannot exceed 20 characters.");
        }
    }
}
