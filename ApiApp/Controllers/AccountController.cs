using ApiApp.API.Services;
using ApiApp.DTOs.UserDtos;
using ApiApp.Models;
using ApiApp.Services;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ApiApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController(
        IValidator<RegisterDto> registerValidator,
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IMapper mapper,
         IConfiguration config,
        JWTService jwtService,
        EmailService emailService
        ) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterDto registerDto)
        {
            var validationResult = registerValidator.Validate(registerDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var user = await userManager.FindByEmailAsync(registerDto.Email);

            if (user != null)
            {
                return BadRequest("User with this email already exists.");
            }

            var newUser = mapper.Map<AppUser>(registerDto);

            var result = await userManager.CreateAsync(newUser, registerDto.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            await userManager.AddToRoleAsync(newUser, "Member");

            var token = await userManager.GenerateEmailConfirmationTokenAsync(newUser);
            var confirmationLink = $"https://localhost:5033/api/Account/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";

            await emailService.SendEmailAsync(newUser.Email,
                                              "Confirm your email",
                                              $"Please confirm your email by clicking <a href='{confirmationLink}'>here</a>.");

            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginDto loginDto)
        {
            var validationResult = new LoginDtoValidator().Validate(loginDto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            var user = await userManager.FindByEmailAsync(loginDto.Email);
            if (user is null) return BadRequest("Invalid username or password");

            var passwordValid = await userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!passwordValid) return BadRequest("Invalid username or password");

            if (!user.EmailConfirmed)
                return BadRequest("Email is not confirmed. Please check your email.");

            var roles = await userManager.GetRolesAsync(user);
            var accessToken = jwtService.GenerateToken(user, roles, config);
            var refreshToken = jwtService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await userManager.UpdateAsync(user);


            return Ok("Login successful");
        }

        [HttpGet("confirmemail")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user is null) return NotFound("User not found");

            var result = await userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return Ok();
        }

        [HttpGet("forgotpassword")]
        public async Task<IActionResult> ForgotPassword([FromQuery] string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user is null) return NotFound("User not found");
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = $"https://localhost:5033/api/Account/reset-password?email={email}&token={Uri.EscapeDataString(token)}";
            await emailService.SendEmailAsync(email,
                                              "Reset your password",
                                              $"You can reset your password by clicking <a href='{resetLink}'>here</a>.");
            return Ok();
        }

        [HttpPost("resetpassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetDto)
        {
            var user = await userManager.FindByEmailAsync(resetDto.Email);
            if (user is null) return NotFound("User not found");
            var result = await userManager.ResetPasswordAsync(user, resetDto.Token, resetDto.NewPassword);
            if (!result.Succeeded) return BadRequest(result.Errors);
            return Ok();
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto refreshDto)
        {
            var principal = jwtService.GetPrincipalFromExpiredToken(refreshDto.Token, config);
            if (principal is null) return BadRequest("Invalid access token");

            var userId = principal.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            var user = await userManager.FindByIdAsync(userId);

            if (user is null || user.RefreshToken != refreshDto.RefreshToken || user.RefreshTokenExpiry <= DateTime.UtcNow)
                return BadRequest("Invalid refresh token");

            var roles = await userManager.GetRolesAsync(user);
            var newAccessToken = jwtService.GenerateToken(user, roles, config);
            var newRefreshToken = jwtService.GenerateRefreshToken();
            var newRefreshTokenExpiry = DateTime.UtcNow.AddDays(3);

            user.RefreshToken = newRefreshToken;
            await userManager.UpdateAsync(user);
            return Ok(new { AccessToken = newAccessToken, RefreshToken = newRefreshToken });
        }

        [HttpPost("revoke")]
        [Authorize]
        public async Task<IActionResult> Revoke()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            var user = await userManager.FindByIdAsync(userId);
            if (user is null) return NotFound("User not found");
            user.RefreshToken = null;
            user.RefreshTokenExpiry = DateTime.MinValue;
            await userManager.UpdateAsync(user);
            return Ok();
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            var user = await userManager.FindByIdAsync(userId);
            var userName = User.Claims.FirstOrDefault(c => c.Type == "userName")?.Value;
            if (user is null) return NotFound("User not found");
            var fullName = User.Claims.FirstOrDefault(c => c.Type == "fullName")?.Value;

            var roles = User.Claims.Where(c => c.Type == "role").Select(c => c.Value).ToList();

            return Ok(new { userId, userName, fullName, roles });
        }
    }

}
