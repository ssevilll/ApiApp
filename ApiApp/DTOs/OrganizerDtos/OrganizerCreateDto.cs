using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace ApiApp.DTOs.OrganizerDtos
{
    public class OrganizerCreateDto
    {
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public IFormFile? Logo { get; set; }
    }

    public class OrganizerCreateDtoValidator : AbstractValidator<OrganizerCreateDto>
    {
        public OrganizerCreateDtoValidator()
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
            RuleFor(o => o.Logo)
                .Must(file => file == null || file.Length > 0).WithMessage("Logo file must not be empty.")
                .Must(file => file == null || file.ContentType.StartsWith("image/")).WithMessage("Logo must be an image file.");
        }
    }
}
