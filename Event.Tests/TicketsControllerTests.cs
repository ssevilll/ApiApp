using ApiApp.Controllers;
using ApiApp.Data;
using ApiApp.DTOs.TicketDtos;
using ApiApp.Models;
using AutoMapper;
using Event.Tests.Helpers;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Event.Tests.Controllers
{
    public class TicketsControllerTests : IAsyncLifetime
    {
        private ApiAppDbContext _context = null!;
        private Mock<IMapper> _mapperMock = null!;
        private Mock<IValidator<TicketCreateDto>> _createValidatorMock = null!;
        private Mock<IValidator<TicketUpdateDto>> _updateValidatorMock = null!;
        private TicketsController _controller = null!;

        public async Task InitializeAsync()
        {
            _context = await DbContextFactory.CreateSeededContextAsync();
            _mapperMock = new Mock<IMapper>();
            _createValidatorMock = new Mock<IValidator<TicketCreateDto>>();
            _updateValidatorMock = new Mock<IValidator<TicketUpdateDto>>();

            _createValidatorMock
                .Setup(v => v.ValidateAsync(It.IsAny<TicketCreateDto>(), default))
                .ReturnsAsync(new ValidationResult());

            _updateValidatorMock
                .Setup(v => v.ValidateAsync(It.IsAny<TicketUpdateDto>(), default))
                .ReturnsAsync(new ValidationResult());

            _controller = new TicketsController(
                _context,
                _mapperMock.Object,
                _createValidatorMock.Object,
                _updateValidatorMock.Object
            );
        }

        public Task DisposeAsync()
        {
            _context.Dispose();
            return Task.CompletedTask;
        }

        // ── GetAll ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetAll_ShouldReturnOk_WithListOfTickets()
        {
            // Seed a ticket so the list is non-empty
            _context.Tickets.Add(new Ticket { Id = 10, EventId = 1, Type = "VIP", Price = 100, QuantityAvailable = 50 });
            await _context.SaveChangesAsync();

            var result = await _controller.GetAll();

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetAll_EmptyDb_ShouldReturnOk()
        {
            var emptyCtx = DbContextFactory.CreateInMemoryContext();
            var ctrl = CreateController(emptyCtx);

            var result = await ctrl.GetAll();

            Assert.IsType<OkObjectResult>(result);
        }

        // ── GetById ────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetById_WhenTicketExists_ShouldReturnOk()
        {
            _context.Tickets.Add(new Ticket { Id = 20, EventId = 1, Type = "General", Price = 50, QuantityAvailable = 100 });
            await _context.SaveChangesAsync();

            var result = await _controller.GetById(20);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetById_WhenTicketDoesNotExist_ShouldReturnNotFound()
        {
            var result = await _controller.GetById(9999);

            Assert.IsType<NotFoundResult>(result);
        }

        // ── Create ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Create_ValidDto_WithExistingEvent_ShouldAddTicketAndReturnOk()
        {
            // Event with Id=1 exists in seeded data
            var dto = new TicketCreateDto { EventId = 1, Type = "General", Price = 25, QuantityAvailable = 200 };
            var entity = new Ticket { EventId = dto.EventId, Type = dto.Type, Price = dto.Price, QuantityAvailable = dto.QuantityAvailable };

            _mapperMock.Setup(m => m.Map<Ticket>(dto)).Returns(entity);

            var result = await _controller.Create(dto);

            Assert.IsType<OkObjectResult>(result);
            Assert.True(_context.Tickets.Any(t => t.Type == "General" && t.EventId == 1));
        }

        [Fact]
        public async Task Create_InvalidDto_ShouldReturnBadRequest()
        {
            var dto = new TicketCreateDto { EventId = 0, Type = "", Price = -1, QuantityAvailable = -5 };

            _createValidatorMock
                .Setup(v => v.ValidateAsync(dto, default))
                .ReturnsAsync(new ValidationResult(new[]
                {
                    new ValidationFailure("EventId", "EventId must be a positive integer."),
                    new ValidationFailure("Type", "Type is required."),
                    new ValidationFailure("Price", "Price must be a non-negative value."),
                    new ValidationFailure("QuantityAvailable", "QuantityAvailable must be a non-negative integer.")
                }));

            var result = await _controller.Create(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Create_NonExistingEvent_ShouldReturnBadRequest()
        {
            var dto = new TicketCreateDto { EventId = 9999, Type = "VIP", Price = 99, QuantityAvailable = 10 };

            _mapperMock.Setup(m => m.Map<Ticket>(dto)).Returns(new Ticket { EventId = 9999, Type = "VIP" });

            var result = await _controller.Create(dto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("9999", badRequest.Value!.ToString());
        }

        // ── Update ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Update_ExistingTicket_ValidDto_ShouldReturnOk()
        {
            _context.Tickets.Add(new Ticket { Id = 30, EventId = 1, Type = "Standard", Price = 40, QuantityAvailable = 80 });
            await _context.SaveChangesAsync();

            var dto = new TicketUpdateDto { Type = "Premium", Price = 60, QuantityAvailable = 70 };

            _mapperMock
                .Setup(m => m.Map(dto, It.IsAny<Ticket>()))
                .Callback<TicketUpdateDto, Ticket>((d, t) =>
                {
                    t.Type = d.Type;
                    t.Price = d.Price;
                    t.QuantityAvailable = d.QuantityAvailable;
                });

            var result = await _controller.Update(30, dto);

            Assert.IsType<OkObjectResult>(result);
            var updated = await _context.Tickets.FindAsync(30);
            Assert.Equal("Premium", updated!.Type);
            Assert.Equal(60, updated.Price);
        }

        [Fact]
        public async Task Update_InvalidDto_ShouldReturnBadRequest()
        {
            var dto = new TicketUpdateDto { Type = "", Price = -10, QuantityAvailable = -1 };

            _updateValidatorMock
                .Setup(v => v.ValidateAsync(dto, default))
                .ReturnsAsync(new ValidationResult(new[]
                {
                    new ValidationFailure("Type", "Type is required."),
                    new ValidationFailure("Price", "Price must be a non-negative value.")
                }));

            var result = await _controller.Update(30, dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Update_NonExistingTicket_ShouldReturnNotFound()
        {
            var dto = new TicketUpdateDto { Type = "VIP", Price = 100, QuantityAvailable = 20 };

            var result = await _controller.Update(9999, dto);

            Assert.IsType<NotFoundResult>(result);
        }

        // ── Delete ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Delete_WhenTicketExists_ShouldRemoveFromDbAndReturnNoContent()
        {
            _context.Tickets.Add(new Ticket { Id = 40, EventId = 1, Type = "Economy", Price = 10, QuantityAvailable = 300 });
            await _context.SaveChangesAsync();

            var result = await _controller.Delete(40);

            Assert.IsType<NoContentResult>(result);
            Assert.Null(await _context.Tickets.FindAsync(40));
        }

        [Fact]
        public async Task Delete_WhenTicketDoesNotExist_ShouldReturnNotFound()
        {
            var result = await _controller.Delete(9999);

            Assert.IsType<NotFoundResult>(result);
        }

        // ── Helper ─────────────────────────────────────────────────────────────

        private TicketsController CreateController(ApiAppDbContext ctx)
        {
            var cv = new Mock<IValidator<TicketCreateDto>>();
            cv.Setup(v => v.ValidateAsync(It.IsAny<TicketCreateDto>(), default))
              .ReturnsAsync(new ValidationResult());

            var uv = new Mock<IValidator<TicketUpdateDto>>();
            uv.Setup(v => v.ValidateAsync(It.IsAny<TicketUpdateDto>(), default))
              .ReturnsAsync(new ValidationResult());

            return new TicketsController(ctx, _mapperMock.Object, cv.Object, uv.Object);
        }
    }
}