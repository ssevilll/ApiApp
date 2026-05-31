using ApiApp.API.Services;
using ApiApp.DTOs.UserDtos;
using ApiApp.Helpers;
using ApiApp.Models;
using ApiApp.Services;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;

namespace ApiApp.Controllers
{
    public class AccountController(
        IValidator<RegisterDto> registerValidator,
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IMapper mapper,
        IConfiguration config,
        JWTService jwtService,
        EmailService emailService
        ) : BaseController
    {
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterDto registerDto)
        {
            var validationResult = registerValidator.Validate(registerDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(ResponseModelHelper.CreateBadRequestResponse<string>("Input is not valid."));
            }

            var user = await userManager.FindByEmailAsync(registerDto.Email);
            if (user != null)
            {
                return BadRequest(ResponseModelHelper.CreateConflictResponse<string>("User with this email already exists."));
            }

            var newUser = mapper.Map<AppUser>(registerDto);

            var result = await userManager.CreateAsync(newUser, registerDto.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(ResponseModelHelper.CreateErrorResponse<string>(errors));
            }

            await userManager.AddToRoleAsync(newUser, "Member");

            var token = await userManager.GenerateEmailConfirmationTokenAsync(newUser);
            var encodedToken = WebUtility.UrlEncode(token);
            var confirmationLink = $"https://localhost:5033/api/Account/confirmemail?email={newUser.Email}&token={encodedToken}";

            await emailService.SendEmailAsync(newUser.Email,
                                              "Confirm your email",
                                              $"Please confirm your email by clicking <a href='{confirmationLink}'>here</a>.");

            var responseData = new { Message = "User registered successfully. Please check your email to confirm your account." };
            return Ok(ResponseModelHelper.CreateSuccessResponse(responseData));
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginDto loginDto)
        {
            var validationResult = new LoginDtoValidator().Validate(loginDto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(ResponseModelHelper.CreateErrorResponse<object>(errors));
            }

            var user = await userManager.FindByEmailAsync(loginDto.Email);
            if (user is null)
                return BadRequest(ResponseModelHelper.CreateBadRequestResponse<object>("Invalid username or password"));

            var passwordValid = await userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!passwordValid)
                return BadRequest(ResponseModelHelper.CreateBadRequestResponse<object>("Invalid username or password"));

            if (!user.EmailConfirmed)
                return BadRequest(ResponseModelHelper.CreateBadRequestResponse<object>("Email is not confirmed. Please check your email."));

            var roles = await userManager.GetRolesAsync(user);
            var accessToken = jwtService.GenerateToken(user, roles, config);
            var refreshToken = jwtService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await userManager.UpdateAsync(user);

            var responseData = new { RefreshToken = refreshToken, AccessToken = accessToken };
            return Ok(ResponseModelHelper.CreateSuccessResponse(responseData));
        }

        [HttpGet("confirmemail")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string email, [FromQuery] string token)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user is null)
                return NotFound(ResponseModelHelper.CreateNotFoundResponse<string>("User not found"));

            var decodedToken = WebUtility.UrlDecode(token);
            var result = await userManager.ConfirmEmailAsync(user, decodedToken);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(ResponseModelHelper.CreateErrorResponse<string>(errors));
            }

            var responseData = new { Message = "Email confirmed successfully" };
            return Ok(ResponseModelHelper.CreateSuccessResponse(responseData));
        }

        [HttpGet("forgotpassword")]
        public async Task<IActionResult> ForgotPassword([FromQuery] string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user is null)
                return NotFound(ResponseModelHelper.CreateNotFoundResponse<string>("User not found"));

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebUtility.UrlEncode(token);
            var resetLink = $"https://localhost:5033/api/Account/resetpassword?email={user.Email}&token={encodedToken}";

            await emailService.SendEmailAsync(email,
                                              "Reset your password",
                                              $"You can reset your password by clicking <a href='{resetLink}'>here</a>.");

            var responseData = new { Message = "Password reset link has been sent to your email." };
            return Ok(ResponseModelHelper.CreateSuccessResponse(responseData));
        }

        [HttpPost("resetpassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetDto)
        {
            var user = await userManager.FindByEmailAsync(resetDto.Email);
            if (user is null)
                return NotFound(ResponseModelHelper.CreateNotFoundResponse<string>("User not found"));

            var result = await userManager.ResetPasswordAsync(user, resetDto.Token, resetDto.NewPassword);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(ResponseModelHelper.CreateErrorResponse<string>(errors));
            }

            var responseData = new { Message = "Password has been reset successfully." };
            return Ok(ResponseModelHelper.CreateSuccessResponse(responseData));
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto refreshDto)
        {
            var principal = jwtService.GetPrincipalFromExpiredToken(refreshDto.Token, config);
            if (principal is null)
                return BadRequest(ResponseModelHelper.CreateBadRequestResponse<object>("Invalid access token"));

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await userManager.FindByIdAsync(userId);

            if (user is null || user.RefreshToken != refreshDto.RefreshToken || user.RefreshTokenExpiry <= DateTime.UtcNow)
                return BadRequest(ResponseModelHelper.CreateBadRequestResponse<object>("Invalid refresh token"));

            var roles = await userManager.GetRolesAsync(user);
            var newAccessToken = jwtService.GenerateToken(user, roles, config);
            var newRefreshToken = jwtService.GenerateRefreshToken();
            var newRefreshTokenExpiry = DateTime.UtcNow.AddDays(3);

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiry = newRefreshTokenExpiry;
            await userManager.UpdateAsync(user);

            var responseData = new { AccessToken = newAccessToken, RefreshToken = newRefreshToken };
            return Ok(ResponseModelHelper.CreateSuccessResponse(responseData));
        }

        [HttpGet("profile")]
        [Authorize]
        public IActionResult GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = User.Identity?.Name;
            var fullName = User.FindFirst("FullName")?.Value;
            var roles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            var responseData = new { UserId = userId, UserName = userName, FullName = fullName, Roles = roles };
            return Ok(ResponseModelHelper.CreateSuccessResponse(responseData));
        }

        [HttpPost("revoke")]
        [Authorize]
        public async Task<IActionResult> RevokeToken()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await userManager.FindByIdAsync(userId);

            if (user is null)
                return NotFound(ResponseModelHelper.CreateNotFoundResponse<string>("User not found"));

            user.RefreshToken = null;
            user.RefreshTokenExpiry = DateTime.MinValue;

            await userManager.UpdateAsync(user);

            var responseData = new { Message = "Token revoked successfully." };
            return Ok(ResponseModelHelper.CreateSuccessResponse(responseData));
        }
    }
}