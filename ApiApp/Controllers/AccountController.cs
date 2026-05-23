using ApiApp.DTOs.UserDtos;
using ApiApp.Models;
using AutoMapper;
using FluentValidation;
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
        IMapper mapper 
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

            var newUser= mapper.Map<AppUser>(registerDto);

            var result = await userManager.CreateAsync(newUser, registerDto.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }


            return Ok();
        }
    }

}
