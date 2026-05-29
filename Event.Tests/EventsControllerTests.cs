using ApiApp.Controllers;
using ApiApp.Data;
using ApiApp.DTOs.EventDtos;
using ApiApp.DTOs.OrganizerDtos;
using ApiApp.DTOs.TicketDtos;
using ApiApp.Interfaces;
using ApiApp.Models;
using AutoMapper;
using Event.Tests.Helpers;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Event.Tests.Controllers
{
    public class EventsControllerTests : IAsyncLifetime
    {
        private ApiAppDbContext _context = null!;
        private Mock<IMapper> _mapperMock = null!;
        private Mock<IFileService> _fileServiceMock = null!;
        private Mock<IValidator<EventCreateDto>> _createValidatorMock = null!;
        private Mock<IValidator<EventUpdateDto>> _updateValidatorMock = null!;
        private EventsController _controller = null!;

        public async Task InitializeAsync()
        {
            _context = await DbContextFactory.CreateSeededContextAsync();
            _mapperMock = new Mock<IMapper>();
            _fileServiceMock = new Mock<IFileService>();
            _createValidatorMock = new Mock<IValidator<EventCreateDto>>();
            _updateValidatorMock = new Mock<IValidator<EventUpdateDto>>();

            // Default: validation passes
            _createValidatorMock
                .Setup(v => v.ValidateAsync(It.IsAny<EventCreateDto>(), default))
                .ReturnsAsync(new ValidationResult());

            _updateValidatorMock
                .Setup(v => v.ValidateAsync(It.IsAny<EventUpdateDto>(), default))
                .ReturnsAsync(new ValidationResult());

            _controller = new EventsController(
                _context,
                _mapperMock.Object,
                _fileServiceMock.Object,
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
        public async Task GetAll_ShouldReturnOk_WithListOfEvents()
        {
            // Two events are seeded
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
        public async Task GetById_WhenEventExists_ShouldReturnOk()
        {
            // Event Id=1 seeded
            var result = await _controller.GetById(1);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetById_WhenEventDoesNotExist_ShouldReturnNotFound()
        {
            var result = await _controller.GetById(9999);

            Assert.IsType<NotFoundResult>(result);
        }

        // ── Create ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Create_ValidDto_WithExistingOrganizer_NoBanner_ShouldAddEventAndReturnOk()
        {
            // Organizer Id=1 exists in seeded data
            var dto = new EventCreateDto
            {
                Title = "New Event",
                Date = DateTime.UtcNow.AddMonths(6),
                Location = "Chicago",
                OrganizerId = 1,
                Banner = null
            };

            var entity = new ApiApp.Models.Event
            {
                Title = dto.Title,
                Date = dto.Date,
                Location = dto.Location,
                OrganizerId = dto.OrganizerId
            };

            _mapperMock.Setup(m => m.Map<ApiApp.Models.Event>(dto)).Returns(entity);

            var result = await _controller.Create(dto);

            Assert.IsType<OkObjectResult>(result);
            Assert.True(_context.Events.Any(e => e.Title == "New Event"));
        }

        [Fact]
        public async Task Create_ValidDto_WithBanner_ShouldSaveBannerAndReturnOk()
        {
            const string bannerUrl = "banners/event.png";
            _fileServiceMock
                .Setup(f => f.SaveFileAsync(It.IsAny<IFormFile>(), "banners"))
                .ReturnsAsync(bannerUrl);

            var bannerMock = new Mock<IFormFile>();
            bannerMock.Setup(f => f.Length).Returns(4096);

            var dto = new EventCreateDto
            {
                Title = "Banner Event",
                Date = DateTime.UtcNow.AddMonths(4),
                Location = "Seattle",
                OrganizerId = 1,
                Banner = bannerMock.Object
            };

            var entity = new ApiApp.Models.Event
            {
                Title = dto.Title,
                Date = dto.Date,
                Location = dto.Location,
                OrganizerId = dto.OrganizerId
            };

            _mapperMock.Setup(m => m.Map<ApiApp.Models.Event>(dto)).Returns(entity);

            var result = await _controller.Create(dto);

            Assert.IsType<OkObjectResult>(result);
            var saved = _context.Events.FirstOrDefault(e => e.Title == "Banner Event");
            Assert.NotNull(saved);
            Assert.Equal(bannerUrl, saved!.BannerImageUrl);
        }

        [Fact]
        public async Task Create_InvalidDto_ShouldReturnBadRequest()
        {
            var dto = new EventCreateDto { Title = "", Location = "", OrganizerId = 0, Date = DateTime.UtcNow.AddDays(-1) };

            _createValidatorMock
                .Setup(v => v.ValidateAsync(dto, default))
                .ReturnsAsync(new ValidationResult(new[]
                {
                    new ValidationFailure("Title", "Title is required."),
                    new ValidationFailure("Location", "Location is required."),
                    new ValidationFailure("OrganizerId", "OrganizerId must be a positive integer."),
                    new ValidationFailure("Date", "Event date must be in the future.")
                }));

            var result = await _controller.Create(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Create_NonExistingOrganizer_ShouldReturnBadRequest()
        {
            var dto = new EventCreateDto
            {
                Title = "Ghost Event",
                Date = DateTime.UtcNow.AddMonths(2),
                Location = "Nowhere",
                OrganizerId = 9999
            };

            _mapperMock.Setup(m => m.Map<ApiApp.Models.Event>(dto))
                       .Returns(new ApiApp.Models.Event { Title = dto.Title, OrganizerId = dto.OrganizerId });

            var result = await _controller.Create(dto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("9999", badRequest.Value!.ToString());
        }

        // ── Update ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Update_ExistingEvent_ValidDto_NoOrganizerChange_ShouldReturnOk()
        {
            var dto = new EventUpdateDto { Title = "Updated Title", Location = "Boston" };

            _mapperMock
                .Setup(m => m.Map(dto, It.IsAny<ApiApp.Models.Event>()))
                .Callback<EventUpdateDto, ApiApp.Models.Event>((d, e) =>
                {
                    if (d.Title != null) e.Title = d.Title;
                    if (d.Location != null) e.Location = d.Location;
                });

            var result = await _controller.Update(1, dto);

            Assert.IsType<OkObjectResult>(result);
            var updated = await _context.Events.FindAsync(1);
            Assert.Equal("Updated Title", updated!.Title);
        }

        [Fact]
        public async Task Update_ExistingEvent_WithValidOrganizerId_ShouldReturnOk()
        {
            // Organizer Id=2 exists in seeded data
            var dto = new EventUpdateDto { OrganizerId = 2 };

            _mapperMock
                .Setup(m => m.Map(dto, It.IsAny<ApiApp.Models.Event>()))
                .Callback<EventUpdateDto, ApiApp.Models.Event>((d, e) =>
                {
                    if (d.OrganizerId.HasValue) e.OrganizerId = d.OrganizerId.Value;
                });

            var result = await _controller.Update(1, dto);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Update_ExistingEvent_WithNonExistingOrganizerId_ShouldReturnBadRequest()
        {
            var dto = new EventUpdateDto { OrganizerId = 9999 };

            var result = await _controller.Update(1, dto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("9999", badRequest.Value!.ToString());
        }

        [Fact]
        public async Task Update_InvalidDto_ShouldReturnBadRequest()
        {
            var dto = new EventUpdateDto { Title = "" };

            _updateValidatorMock
                .Setup(v => v.ValidateAsync(dto, default))
                .ReturnsAsync(new ValidationResult(new[]
                {
                    new ValidationFailure("Title", "Title cannot be empty if provided.")
                }));

            var result = await _controller.Update(1, dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Update_NonExistingEvent_ShouldReturnNotFound()
        {
            var dto = new EventUpdateDto { Title = "Ghost" };

            var result = await _controller.Update(9999, dto);

            Assert.IsType<NotFoundResult>(result);
        }

        // ── Delete ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Delete_WhenEventExists_ShouldRemoveFromDbAndReturnNoContent()
        {
            var ev = new ApiApp.Models.Event
            {
                Id = 50,
                Title = "To Delete",
                Date = DateTime.UtcNow.AddMonths(1),
                Location = "Temp",
                OrganizerId = 1,
                BannerImageUrl = "banners/delete.png"
            };
            _context.Events.Add(ev);
            await _context.SaveChangesAsync();

            var result = await _controller.Delete(50);

            Assert.IsType<NoContentResult>(result);
            Assert.Null(await _context.Events.FindAsync(50));
            _fileServiceMock.Verify(f => f.DeleteFile("banners/delete.png"), Times.Once);
        }

        [Fact]
        public async Task Delete_WhenEventDoesNotExist_ShouldReturnNotFound()
        {
            var result = await _controller.Delete(9999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_EventWithNullBanner_ShouldCallDeleteFileWithNull()
        {
            var ev = new ApiApp.Models.Event
            {
                Id = 51,
                Title = "No Banner",
                Date = DateTime.UtcNow.AddMonths(1),
                Location = "Somewhere",
                OrganizerId = 1,
                BannerImageUrl = null
            };
            _context.Events.Add(ev);
            await _context.SaveChangesAsync();

            await _controller.Delete(51);

            _fileServiceMock.Verify(f => f.DeleteFile(null), Times.Once);
        }

        // ── UploadBanner ───────────────────────────────────────────────────────

        [Fact]
        public async Task UploadBanner_ValidFile_ShouldReturnOkAndUpdateBannerUrl()
        {
            const string newUrl = "banners/new.png";
            _fileServiceMock
                .Setup(f => f.SaveFileAsync(It.IsAny<IFormFile>(), "banners"))
                .ReturnsAsync(newUrl);

            var bannerMock = new Mock<IFormFile>();
            bannerMock.Setup(f => f.Length).Returns(2048);

            var result = await _controller.UploadBanner(1, bannerMock.Object);

            Assert.IsType<OkObjectResult>(result);
            var ev = await _context.Events.FindAsync(1);
            Assert.Equal(newUrl, ev!.BannerImageUrl);
        }

        [Fact]
        public async Task UploadBanner_NullFile_ShouldReturnBadRequest()
        {
            var result = await _controller.UploadBanner(1, null!);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UploadBanner_ZeroLengthFile_ShouldReturnBadRequest()
        {
            var bannerMock = new Mock<IFormFile>();
            bannerMock.Setup(f => f.Length).Returns(0);

            var result = await _controller.UploadBanner(1, bannerMock.Object);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UploadBanner_NonExistingEvent_ShouldReturnNotFound()
        {
            var bannerMock = new Mock<IFormFile>();
            bannerMock.Setup(f => f.Length).Returns(1024);

            var result = await _controller.UploadBanner(9999, bannerMock.Object);

            Assert.IsType<NotFoundResult>(result);
        }

        // ── GetTickets ─────────────────────────────────────────────────────────

        [Fact]
        public async Task GetTickets_ExistingEvent_ShouldReturnOk()
        {
            var result = await _controller.GetTickets(1);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetTickets_NonExistingEvent_ShouldReturnNotFound()
        {
            var result = await _controller.GetTickets(9999);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        // ── GetOrganizer ───────────────────────────────────────────────────────

        [Fact]
        public async Task GetOrganizer_ExistingEvent_ShouldReturnOk()
        {
            var result = await _controller.GetOrganizer(1);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetOrganizer_NonExistingEvent_ShouldReturnNotFound()
        {
            var result = await _controller.GetOrganizer(9999);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        // ── Helper ─────────────────────────────────────────────────────────────

        private EventsController CreateController(ApiAppDbContext ctx)
        {
            var cv = new Mock<IValidator<EventCreateDto>>();
            cv.Setup(v => v.ValidateAsync(It.IsAny<EventCreateDto>(), default))
              .ReturnsAsync(new ValidationResult());

            var uv = new Mock<IValidator<EventUpdateDto>>();
            uv.Setup(v => v.ValidateAsync(It.IsAny<EventUpdateDto>(), default))
              .ReturnsAsync(new ValidationResult());

            return new EventsController(ctx, _mapperMock.Object, _fileServiceMock.Object, cv.Object, uv.Object);
        }
    }
}