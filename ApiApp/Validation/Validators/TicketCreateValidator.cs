using ApiApp.DTOs.TicketDtos;
using FluentValidation;

namespace ApiApp.Validation.Validators
{
    public class TicketCreateValidator : AbstractValidator<TicketCreateDto>
    {
        public TicketCreateValidator()
        {
            RuleFor(x => x.Type)
                .NotEmpty().WithMessage("Ticket type is required.")
                .MaximumLength(50).WithMessage("Type must not exceed 50 characters.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be a positive value.");

            RuleFor(x => x.QuantityAvailable)
                .GreaterThanOrEqualTo(0).WithMessage("Quantity must be non-negative.");

            RuleFor(x => x.EventId)
                .GreaterThan(0).WithMessage("A valid EventId is required.");
        }
    }
}
