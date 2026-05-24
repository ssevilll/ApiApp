using FluentValidation;

namespace ApiApp.DTOs.EventDtos
{
    public class EventUpdateDto
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime Date { get; set; }
        public string Location { get; set; } = null!;
        public int OrganizerId { get; set; }
    }
    public class EventUpdateDtoValidator : AbstractValidator<EventUpdateDto>
    {
        public EventUpdateDtoValidator()
        {
            RuleFor(e => e.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(150).WithMessage("Title cannot exceed 150 characters.");
            RuleFor(e => e.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");
            RuleFor(e => e.Date)
                .GreaterThan(DateTime.Now).WithMessage("Event date must be in the future.");
            RuleFor(e => e.Location)
                .NotEmpty().WithMessage("Location is required.")
                .MaximumLength(200).WithMessage("Location cannot exceed 200 characters.");
            RuleFor(e => e.OrganizerId)
                .GreaterThan(0).WithMessage("OrganizerId must be a positive integer.");
        }
    }
}
