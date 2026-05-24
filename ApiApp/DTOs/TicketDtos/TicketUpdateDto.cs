using FluentValidation;

namespace ApiApp.DTOs.TicketDtos
{
    public class TicketUpdateDto
    {
        public string Type { get; set; } = null!;
        public decimal Price { get; set; }
        public int QuantityAvailable { get; set; }
    }

    public class TicketUpdateDtoValidator : AbstractValidator<TicketUpdateDto>
    {
        public TicketUpdateDtoValidator()
        {
            RuleFor(t => t.Type)
                .NotEmpty().WithMessage("Type is required.")
                .MaximumLength(50).WithMessage("Type cannot exceed 50 characters.");
            RuleFor(t => t.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Price must be a non-negative value.");
            RuleFor(t => t.QuantityAvailable)
                .GreaterThanOrEqualTo(0).WithMessage("QuantityAvailable must be a non-negative integer.");
        }
    }
}
