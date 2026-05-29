using ApiApp.API.Services;
using ApiApp.Controllers;
using ApiApp.DTOs.UserDtos;
using ApiApp.Models;
using ApiApp.Services;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Security.Claims;

namespace Event.Tests.Controllers
{
    public class AccountControllerTests
    {
        private readonly Mock<IValidator<RegisterDto>> _registerValidatorMock;
        private readonly Mock<UserManager<AppUser>> _userManagerMock;
        private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IConfiguration> _configMock;
        private readonly Mock<JWTService> _jwtServiceMock;
        private readonly Mock<EmailService> _emailServiceMock;
        private readonly AccountController _controller;

        public AccountControllerTests()
        {
            _registerValidatorMock = new Mock<IValidator<RegisterDto>>();

            // UserManager requires a store mock
            var userStoreMock = new Mock<IUserStore<AppUser>>();
            _userManagerMock = new Mock<UserManager<AppUser>>(
                userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
            _roleManagerMock = new Mock<RoleManager<IdentityRole>>(
                roleStoreMock.Object, null!, null!, null!, null!);

            _mapperMock = new Mock<IMapper>();
            _configMock = new Mock<IConfiguration>();
            _jwtServiceMock = new Mock<JWTService>();
            _emailServiceMock = new Mock<EmailService>(_configMock.Object);

            // Default: register validation passes
            _registerValidatorMock
                .Setup(v => v.Validate(It.IsAny<RegisterDto>()))
                .Returns(new ValidationResult());

            _controller = new AccountController(
                _registerValidatorMock.Object,
                _userManagerMock.Object,
                _roleManagerMock.Object,
                _mapperMock.Object,
                _configMock.Object,
                _jwtServiceMock.Object,
                _emailServiceMock.Object
            );
        }

        // ── Register ───────────────────────────────────────────────────────────

        [Fact]
        public async Task Register_InvalidDto_ShouldReturnBadRequest()
        {
            var dto = new RegisterDto { FullName = "", Email = "bad", Password = "123", Username = "" };

            _registerValidatorMock
                .Setup(v => v.Validate(dto))
                .Returns(new ValidationResult(new[]
                {
                    new ValidationFailure("FullName", "Full name is required."),
                    new ValidationFailure("Email", "Invalid email format."),
                    new ValidationFailure("Password", "Password must be at least 6 characters long.")
                }));

            var result = await _controller.RegisterAsync(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Register_EmailAlreadyInUse_ShouldReturnBadRequest()
        {
            var dto = new RegisterDto { FullName = "John", Email = "existing@test.com", Password = "Pass123!", Username = "john", ConfirmPassword = "Pass123!" };
            var existingUser = new AppUser { Email = dto.Email };

            _userManagerMock.Setup(u => u.FindByEmailAsync(dto.Email)).ReturnsAsync(existingUser);

            var result = await _controller.RegisterAsync(dto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("already exists", badRequest.Value!.ToString());
        }

        [Fact]
        public async Task Register_CreateUserFails_ShouldReturnBadRequest()
        {
            var dto = new RegisterDto { FullName = "John", Email = "new@test.com", Password = "Pass123!", Username = "john", ConfirmPassword = "Pass123!" };
            var newUser = new AppUser { Email = dto.Email, FullName = dto.FullName };

            _userManagerMock.Setup(u => u.FindByEmailAsync(dto.Email)).ReturnsAsync((AppUser?)null);
            _mapperMock.Setup(m => m.Map<AppUser>(dto)).Returns(newUser);
            _userManagerMock
                .Setup(u => u.CreateAsync(newUser, dto.Password))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Passwords must have at least one non alphanumeric character." }));

            var result = await _controller.RegisterAsync(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Register_Success_ShouldSendEmailAndReturnOk()
        {
            var dto = new RegisterDto { FullName = "John", Email = "john@test.com", Password = "Pass123!", Username = "john", ConfirmPassword = "Pass123!" };
            var newUser = new AppUser { Email = dto.Email, FullName = dto.FullName };

            _userManagerMock.Setup(u => u.FindByEmailAsync(dto.Email)).ReturnsAsync((AppUser?)null);
            _mapperMock.Setup(m => m.Map<AppUser>(dto)).Returns(newUser);
            _userManagerMock.Setup(u => u.CreateAsync(newUser, dto.Password)).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(u => u.AddToRoleAsync(newUser, "Member")).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(u => u.GenerateEmailConfirmationTokenAsync(newUser)).ReturnsAsync("confirm-token");
            _emailServiceMock.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                             .Returns(Task.CompletedTask);

            var result = await _controller.RegisterAsync(dto);

            Assert.IsType<OkObjectResult>(result);
            _emailServiceMock.Verify(e => e.SendEmailAsync(dto.Email, "Confirm your email", It.IsAny<string>()), Times.Once);
        }

        // ── Login ──────────────────────────────────────────────────────────────

        [Fact]
        public async Task Login_InvalidDto_ShouldReturnBadRequest()
        {
            // Empty email + empty password — LoginDtoValidator will fail
            var dto = new LoginDto { Email = "", Password = "" };

            var result = await _controller.LoginAsync(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Login_UserNotFound_ShouldReturnBadRequest()
        {
            var dto = new LoginDto { Email = "ghost@test.com", Password = "Pass123!" };

            _userManagerMock.Setup(u => u.FindByEmailAsync(dto.Email)).ReturnsAsync((AppUser?)null);

            var result = await _controller.LoginAsync(dto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Invalid username or password", badRequest.Value!.ToString());
        }

        [Fact]
        public async Task Login_WrongPassword_ShouldReturnBadRequest()
        {
            var dto = new LoginDto { Email = "user@test.com", Password = "WrongPass!" };
            var user = new AppUser { Email = dto.Email, EmailConfirmed = true };

            _userManagerMock.Setup(u => u.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
            _userManagerMock.Setup(u => u.CheckPasswordAsync(user, dto.Password)).ReturnsAsync(false);

            var result = await _controller.LoginAsync(dto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Invalid username or password", badRequest.Value!.ToString());
        }

        [Fact]
        public async Task Login_EmailNotConfirmed_ShouldReturnBadRequest()
        {
            var dto = new LoginDto { Email = "unconfirmed@test.com", Password = "Pass123!" };
            var user = new AppUser { Email = dto.Email, EmailConfirmed = false };

            _userManagerMock.Setup(u => u.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
            _userManagerMock.Setup(u => u.CheckPasswordAsync(user, dto.Password)).ReturnsAsync(true);

            var result = await _controller.LoginAsync(dto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("not confirmed", badRequest.Value!.ToString());
        }

        [Fact]
        public async Task Login_Success_ShouldReturnOkWithTokens()
        {
            var dto = new LoginDto { Email = "valid@test.com", Password = "Pass123!" };
            var user = new AppUser { Email = dto.Email, EmailConfirmed = true, UserName = "validuser" };

            _userManagerMock.Setup(u => u.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
            _userManagerMock.Setup(u => u.CheckPasswordAsync(user, dto.Password)).ReturnsAsync(true);
            _userManagerMock.Setup(u => u.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Member" });
            _jwtServiceMock.Setup(j => j.GenerateToken(user, It.IsAny<IList<string>>(), _configMock.Object)).Returns("access-token");
            _jwtServiceMock.Setup(j => j.GenerateRefreshToken()).Returns("refresh-token");
            _userManagerMock.Setup(u => u.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            var result = await _controller.LoginAsync(dto);

            Assert.IsType<OkObjectResult>(result);
        }

        // ── ConfirmEmail ───────────────────────────────────────────────────────

        [Fact]
        public async Task ConfirmEmail_UserNotFound_ShouldReturnNotFound()
        {
            _userManagerMock.Setup(u => u.FindByEmailAsync("ghost@test.com")).ReturnsAsync((AppUser?)null);

            var result = await _controller.ConfirmEmail("ghost@test.com", "some-token");

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task ConfirmEmail_InvalidToken_ShouldReturnBadRequest()
        {
            var user = new AppUser { Email = "user@test.com" };

            _userManagerMock.Setup(u => u.FindByEmailAsync(user.Email)).ReturnsAsync(user);
            _userManagerMock.Setup(u => u.ConfirmEmailAsync(user, It.IsAny<string>()))
                            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Invalid token." }));

            var result = await _controller.ConfirmEmail(user.Email, "bad-token");

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task ConfirmEmail_ValidToken_ShouldReturnOk()
        {
            var user = new AppUser { Email = "user@test.com" };

            _userManagerMock.Setup(u => u.FindByEmailAsync(user.Email)).ReturnsAsync(user);
            _userManagerMock.Setup(u => u.ConfirmEmailAsync(user, It.IsAny<string>()))
                            .ReturnsAsync(IdentityResult.Success);

            var result = await _controller.ConfirmEmail(user.Email, "valid-token");

            Assert.IsType<OkObjectResult>(result);
        }

        // ── ForgotPassword ─────────────────────────────────────────────────────

        [Fact]
        public async Task ForgotPassword_UserNotFound_ShouldReturnNotFound()
        {
            _userManagerMock.Setup(u => u.FindByEmailAsync("ghost@test.com")).ReturnsAsync((AppUser?)null);

            var result = await _controller.ForgotPassword("ghost@test.com");

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task ForgotPassword_UserExists_ShouldSendEmailAndReturnOk()
        {
            var user = new AppUser { Email = "user@test.com" };

            _userManagerMock.Setup(u => u.FindByEmailAsync(user.Email)).ReturnsAsync(user);
            _userManagerMock.Setup(u => u.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("reset-token");
            _emailServiceMock.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                             .Returns(Task.CompletedTask);

            var result = await _controller.ForgotPassword(user.Email);

            Assert.IsType<OkObjectResult>(result);
            _emailServiceMock.Verify(e => e.SendEmailAsync(user.Email, "Reset your password", It.IsAny<string>()), Times.Once);
        }

        // ── ResetPassword ──────────────────────────────────────────────────────

        [Fact]
        public async Task ResetPassword_UserNotFound_ShouldReturnNotFound()
        {
            var dto = new ResetPasswordDto { Email = "ghost@test.com", Token = "token", NewPassword = "NewPass123!" };

            _userManagerMock.Setup(u => u.FindByEmailAsync(dto.Email)).ReturnsAsync((AppUser?)null);

            var result = await _controller.ResetPassword(dto);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task ResetPassword_InvalidToken_ShouldReturnBadRequest()
        {
            var dto = new ResetPasswordDto { Email = "user@test.com", Token = "bad-token", NewPassword = "NewPass123!" };
            var user = new AppUser { Email = dto.Email };

            _userManagerMock.Setup(u => u.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
            _userManagerMock.Setup(u => u.ResetPasswordAsync(user, dto.Token, dto.NewPassword))
                            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Invalid token." }));

            var result = await _controller.ResetPassword(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task ResetPassword_Success_ShouldReturnOk()
        {
            var dto = new ResetPasswordDto { Email = "user@test.com", Token = "valid-token", NewPassword = "NewPass123!" };
            var user = new AppUser { Email = dto.Email };

            _userManagerMock.Setup(u => u.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
            _userManagerMock.Setup(u => u.ResetPasswordAsync(user, dto.Token, dto.NewPassword))
                            .ReturnsAsync(IdentityResult.Success);

            var result = await _controller.ResetPassword(dto);

            Assert.IsType<OkObjectResult>(result);
        }

        // ── Refresh ────────────────────────────────────────────────────────────

        [Fact]
        public async Task Refresh_InvalidAccessToken_PrincipalIsNull_ShouldReturnBadRequest()
        {
            var dto = new RefreshTokenDto { Token = "bad-access-token", RefreshToken = "some-refresh" };

            _jwtServiceMock.Setup(j => j.GetPrincipalFromExpiredToken(dto.Token, _configMock.Object))
                           .Returns((ClaimsPrincipal?)null);

            var result = await _controller.Refresh(dto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Invalid access token", badRequest.Value!.ToString());
        }

        [Fact]
        public async Task Refresh_UserNotFound_ShouldReturnBadRequest()
        {
            var dto = new RefreshTokenDto { Token = "access-token", RefreshToken = "refresh-token" };
            var claims = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "user-id-123")
            }));

            _jwtServiceMock.Setup(j => j.GetPrincipalFromExpiredToken(dto.Token, _configMock.Object))
                           .Returns(claims);
            _userManagerMock.Setup(u => u.FindByIdAsync("user-id-123")).ReturnsAsync((AppUser?)null);

            var result = await _controller.Refresh(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Refresh_RefreshTokenMismatch_ShouldReturnBadRequest()
        {
            var dto = new RefreshTokenDto { Token = "access-token", RefreshToken = "wrong-refresh-token" };
            var user = new AppUser { Id = "user-id-123", RefreshToken = "correct-refresh-token", RefreshTokenExpiry = DateTime.UtcNow.AddDays(1) };
            var claims = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            }));

            _jwtServiceMock.Setup(j => j.GetPrincipalFromExpiredToken(dto.Token, _configMock.Object))
                           .Returns(claims);
            _userManagerMock.Setup(u => u.FindByIdAsync(user.Id)).ReturnsAsync(user);

            var result = await _controller.Refresh(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Refresh_RefreshTokenExpired_ShouldReturnBadRequest()
        {
            var dto = new RefreshTokenDto { Token = "access-token", RefreshToken = "expired-refresh" };
            var user = new AppUser { Id = "user-id-123", RefreshToken = "expired-refresh", RefreshTokenExpiry = DateTime.UtcNow.AddDays(-1) };
            var claims = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            }));

            _jwtServiceMock.Setup(j => j.GetPrincipalFromExpiredToken(dto.Token, _configMock.Object))
                           .Returns(claims);
            _userManagerMock.Setup(u => u.FindByIdAsync(user.Id)).ReturnsAsync(user);

            var result = await _controller.Refresh(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Refresh_ValidRequest_ShouldReturnOkWithNewTokens()
        {
            var dto = new RefreshTokenDto { Token = "access-token", RefreshToken = "valid-refresh" };
            var user = new AppUser { Id = "user-id-123", RefreshToken = "valid-refresh", RefreshTokenExpiry = DateTime.UtcNow.AddDays(5) };
            var claims = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            }));

            _jwtServiceMock.Setup(j => j.GetPrincipalFromExpiredToken(dto.Token, _configMock.Object))
                           .Returns(claims);
            _userManagerMock.Setup(u => u.FindByIdAsync(user.Id)).ReturnsAsync(user);
            _userManagerMock.Setup(u => u.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Member" });
            _jwtServiceMock.Setup(j => j.GenerateToken(user, It.IsAny<IList<string>>(), _configMock.Object)).Returns("new-access-token");
            _jwtServiceMock.Setup(j => j.GenerateRefreshToken()).Returns("new-refresh-token");
            _userManagerMock.Setup(u => u.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            var result = await _controller.Refresh(dto);

            Assert.IsType<OkObjectResult>(result);
        }

        // ── RevokeToken ────────────────────────────────────────────────────────

        [Fact]
        public async Task RevokeToken_UserNotFound_ShouldReturnNotFound()
        {
            var userId = "missing-user-id";
            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            }));
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            _userManagerMock.Setup(u => u.FindByIdAsync(userId)).ReturnsAsync((AppUser?)null);

            var result = await _controller.RevokeToken();

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task RevokeToken_UserExists_ShouldClearTokenAndReturnOk()
        {
            var userId = "user-id-123";
            var user = new AppUser { Id = userId, RefreshToken = "some-token", RefreshTokenExpiry = DateTime.UtcNow.AddDays(3) };

            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            }));
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            _userManagerMock.Setup(u => u.FindByIdAsync(userId)).ReturnsAsync(user);
            _userManagerMock.Setup(u => u.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            var result = await _controller.RevokeToken();

            Assert.IsType<OkObjectResult>(result);
            Assert.Null(user.RefreshToken);
            Assert.Equal(DateTime.MinValue, user.RefreshTokenExpiry);
        }
    }
}