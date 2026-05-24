using ApiApp.Controllers;
using ApiApp.DTOs.OrganizerDtos;
using ApiApp.Interfaces;
using ApiApp.Models;
using AutoMapper;
using Event.Tests.Helpers;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
using Moq;

namespace Event.Tests.Controllers
{
    public class OrganizersControllerTests : IAsyncLifetime
    {
        private ApiApp.Data.ApiAppDbContext _context = null!;
        private Mock<IMapper> _mapperMock = null!;
        private Mock<IFileService> _fileServiceMock = null!;
        private Mock<IValidator<OrganizerCreateDto>> _createValidatorMock = null!;
        private Mock<IValidator<OrganizerUpdateDto>> _updateValidatorMock = null!;
        private OrganizersController _controller = null!;

        public async Task InitializeAsync()
        {
            _context = await DbContextFactory.CreateSeededContextAsync();
            _mapperMock = new Mock<IMapper>();
            _fileServiceMock = new Mock<IFileService>();
            _createValidatorMock = new Mock<IValidator<OrganizerCreateDto>>();
            _updateValidatorMock = new Mock<IValidator<OrganizerUpdateDto>>();

            // Default: validation passes
            _createValidatorMock
                .Setup(v => v.ValidateAsync(It.IsAny<OrganizerCreateDto>(), default))
                .ReturnsAsync(new ValidationResult());

            _updateValidatorMock
                .Setup(v => v.ValidateAsync(It.IsAny<OrganizerUpdateDto>(), default))
                .ReturnsAsync(new ValidationResult());

            _controller = new OrganizersController(
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
        public async Task GetAll_ShouldReturnOk_WithListOfOrganizers()
        {
            var result = await _controller.GetAll();

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task GetAll_EmptyDb_ShouldReturnOk()
        {
            var emptyCtx = DbContextFactory.CreateInMemoryContext();
            var ctrl = CreateController(emptyCtx);

            var result = await ctrl.GetAll();

            Assert.IsType<OkResult>(result);
        }

        // ── GetById ────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetById_WhenOrganizerExists_ShouldReturnOk()
        {
            // Organizer with Id=1 is seeded by CreateSeededContextAsync
            var result = await _controller.GetById(1);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task GetById_WhenOrganizerDoesNotExist_ShouldReturnNotFound()
        {
            var result = await _controller.GetById(999);

            Assert.IsType<NotFoundResult>(result);
        }

        // ── Create ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Create_ValidDto_ShouldAddOrganizerAndReturnOk()
        {
            var dto = new OrganizerCreateDto
            {
                Name = "New Organizer",
                Email = "neworg@test.com",
                Phone = "555"
            };

            var entity = new Organizer
            {
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone
            };

            _mapperMock
                .Setup(m => m.Map<Organizer>(dto))
                .Returns(entity);

            var result = await _controller.Create(dto);

            Assert.IsType<OkResult>(result);
            Assert.True(_context.Organizers.Any(o => o.Email == "neworg@test.com"));
        }

        [Fact]
        public async Task Create_DuplicateEmail_ShouldReturnConflict()
        {
            // Organizer with Email "tech@corp.com" already exists in seeded data
            var dto = new OrganizerCreateDto
            {
                Name = "Duplicate",
                Email = "tech@corp.com",
                Phone = "000"
            };

            _mapperMock
                .Setup(m => m.Map<Organizer>(dto))
                .Returns(new Organizer { Name = dto.Name, Email = dto.Email });

            var result = await _controller.Create(dto);

            Assert.IsType<ConflictObjectResult>(result);
        }

        // ── Update ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Update_ExistingOrganizer_ShouldReturnOk()
        {
            var dto = new OrganizerUpdateDto
            {
                Name = "Updated Name",
                Email = "updated@test.com",
                Phone = "999"
            };

            _mapperMock
                .Setup(m => m.Map(dto, It.IsAny<Organizer>()))
                .Callback<OrganizerUpdateDto, Organizer>((d, o) =>
                {
                    o.Name = d.Name;
                    o.Email = d.Email;
                    o.Phone = d.Phone;
                });

            var result = await _controller.Update(1, dto);

            Assert.IsType<OkResult>(result);

            var updated = await _context.Organizers.FindAsync(1);
            Assert.Equal("Updated Name", updated!.Name);
            Assert.Equal("updated@test.com", updated.Email);
        }

        [Fact]
        public async Task Update_NonExistingOrganizer_ShouldReturnNotFound()
        {
            var result = await _controller.Update(9999, new OrganizerUpdateDto
            {
                Name = "Ghost",
                Email = "ghost@test.com"
            });

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Update_DuplicateEmailOnAnotherOrganizer_ShouldReturnConflict()
        {
            // Organizer 2 has email "info@music.com" — try to assign it to organizer 1
            var dto = new OrganizerUpdateDto
            {
                Name = "Tech Corp",
                Email = "info@music.com",
                Phone = "111"
            };

            var result = await _controller.Update(1, dto);

            Assert.IsType<ConflictObjectResult>(result);
        }


        // ── Delete ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Delete_WhenOrganizerExists_ShouldRemoveFromDbAndReturnNoContent()
        {
            var org = new Organizer
            {
                Id = 50,
                Name = "To Delete",
                Email = "del@test.com",
                Phone = "000",
                LogoUrl = "logos/del.png"
            };
            _context.Organizers.Add(org);
            await _context.SaveChangesAsync();

            var result = await _controller.Delete(50);

            Assert.IsType<NoContentResult>(result);
            Assert.Null(await _context.Organizers.FindAsync(50));
            _fileServiceMock.Verify(f => f.DeleteFile("logos/del.png"), Times.Once);
        }

        [Fact]
        public async Task Delete_WhenOrganizerDoesNotExist_ShouldReturnNotFound()
        {
            var result = await _controller.Delete(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_OrganizerWithNullLogo_ShouldCallDeleteFileWithNull()
        {
            var org = new Organizer
            {
                Id = 51,
                Name = "No Logo Org",
                Email = "nolog@test.com",
                LogoUrl = null
            };
            _context.Organizers.Add(org);
            await _context.SaveChangesAsync();

            await _controller.Delete(51);

            _fileServiceMock.Verify(f => f.DeleteFile(null), Times.Once);
        }

        // ── UploadLogo ─────────────────────────────────────────────────────────

        [Fact]
        public async Task UploadLogo_ValidFile_ShouldReturnOkAndUpdateLogoUrl()
        {
            const string newUrl = "logos/new.png";
            _fileServiceMock
                .Setup(f => f.SaveFileAsync(It.IsAny<IFormFile>(), "logos"))
                .ReturnsAsync(newUrl);

            var logoMock = new Mock<IFormFile>();
            logoMock.Setup(f => f.Length).Returns(2048);

            var result = await _controller.UploadLogo(1, logoMock.Object);

            Assert.IsType<OkResult>(result);
            var org = await _context.Organizers.FindAsync(1);
            Assert.Equal(newUrl, org!.LogoUrl);
        }

        [Fact]
        public async Task UploadLogo_NullFile_ShouldReturnBadRequest()
        {
            var result = await _controller.UploadLogo(1, null!);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UploadLogo_NonExistingOrganizer_ShouldReturnNotFound()
        {
            var logoMock = new Mock<IFormFile>();
            logoMock.Setup(f => f.Length).Returns(512);

            var result = await _controller.UploadLogo(9999, logoMock.Object);

            Assert.IsType<NotFoundResult>(result);
        }

        // ── GetEvents ──────────────────────────────────────────────────────────

        [Fact]
        public async Task GetEvents_ExistingOrganizer_ShouldReturnOk()
        {
            var result = await _controller.GetEvents(1);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task GetEvents_NonExistingOrganizer_ShouldReturnNotFound()
        {
            var result = await _controller.GetEvents(9999);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        // ── Helper ─────────────────────────────────────────────────────────────

        private OrganizersController CreateController(ApiApp.Data.ApiAppDbContext ctx)
        {
            var cv = new Mock<IValidator<OrganizerCreateDto>>();
            cv.Setup(v => v.ValidateAsync(It.IsAny<OrganizerCreateDto>(), default))
              .ReturnsAsync(new ValidationResult());

            var uv = new Mock<IValidator<OrganizerUpdateDto>>();
            uv.Setup(v => v.ValidateAsync(It.IsAny<OrganizerUpdateDto>(), default))
              .ReturnsAsync(new ValidationResult());

            return new OrganizersController(
                ctx,
                _mapperMock.Object,
                _fileServiceMock.Object,
                cv.Object,
                uv.Object
            );
        }
    }
}