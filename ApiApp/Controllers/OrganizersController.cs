using ApiApp.Data;
using ApiApp.DTOs.EventDtos;
using ApiApp.DTOs.OrganizerDtos;
using ApiApp.Helpers;
using ApiApp.Interfaces;
using ApiApp.Models;
using ApiApp.Services;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiApp.Controllers
{
    public class OrganizersController(
        ApiAppDbContext _context,
        IMapper _mapper,
        IFileService _fileService,
        IValidator<OrganizerCreateDto> _createValidator,
        IValidator<OrganizerUpdateDto> _updateValidator
        ) : BaseController
    {

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var organizers = await _context.Organizers.ToListAsync();
            var response = _mapper.Map<List<OrganizerResponseDto>>(organizers);
            return Ok(ResponseModelHelper.CreateSuccessResponse(response));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var organizer = await _context.Organizers.FindAsync(id);
            if (organizer == null)
                return NotFound(ResponseModelHelper.CreateNotFoundResponse<string>($"Organizer with Id {id} was not found."));

            var response = _mapper.Map<OrganizerResponseDto>(organizer);
            return Ok(ResponseModelHelper.CreateSuccessResponse(response));
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create([FromForm] OrganizerCreateDto dto)
        {
            var validation = await _createValidator.ValidateAsync(dto);
            if (!validation.IsValid)
            {
                var errors = validation.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(ResponseModelHelper.CreateErrorResponse<object>(errors));
            }

            var emailInUse = await _context.Organizers.AnyAsync(o => o.Email == dto.Email);
            if (emailInUse)
                return Conflict(ResponseModelHelper.CreateConflictResponse<string>("An organizer with this email already exists."));

            var organizer = _mapper.Map<Organizer>(dto);

            if (dto.Logo != null && dto.Logo.Length > 0)
            {
                organizer.LogoUrl = await _fileService.SaveFileAsync(dto.Logo, "logos");
            }

            _context.Organizers.Add(organizer);
            await _context.SaveChangesAsync();

            var responseData = new { Message = "Organizer created successfully." };
            return Ok(ResponseModelHelper.CreateSuccessResponse(responseData));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Update(int id, OrganizerUpdateDto dto)
        {
            var validation = await _updateValidator.ValidateAsync(dto);
            if (!validation.IsValid)
            {
                var errors = validation.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(ResponseModelHelper.CreateErrorResponse<object>(errors));
            }

            var organizer = await _context.Organizers.FindAsync(id);
            if (organizer == null)
                return NotFound(ResponseModelHelper.CreateNotFoundResponse<string>($"Organizer with Id {id} was not found."));

            var emailInUse = await _context.Organizers
                .AnyAsync(o => o.Email == dto.Email && o.Id != id);
            if (emailInUse)
                return Conflict(ResponseModelHelper.CreateConflictResponse<string>("Another organizer with this email already exists."));

            _mapper.Map(dto, organizer);
            await _context.SaveChangesAsync();

            var responseData = new { Message = "Organizer updated successfully." };
            return Ok(ResponseModelHelper.CreateSuccessResponse(responseData));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var organizer = await _context.Organizers.FindAsync(id);
            if (organizer == null)
                return NotFound(ResponseModelHelper.CreateNotFoundResponse<string>($"Organizer with Id {id} was not found."));

            _fileService.DeleteFile(organizer.LogoUrl);
            _context.Organizers.Remove(organizer);
            await _context.SaveChangesAsync();

            var responseData = new { Message = $"Organizer {id} was deleted successfully." };
            return Ok(ResponseModelHelper.CreateSuccessResponse(responseData));
        }

        [HttpPost("{id}/logo")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UploadLogo(int id, IFormFile logo)
        {
            if (logo == null || logo.Length == 0)
                return BadRequest(ResponseModelHelper.CreateBadRequestResponse<string>("No file uploaded."));

            var organizer = await _context.Organizers.FindAsync(id);
            if (organizer == null)
                return NotFound(ResponseModelHelper.CreateNotFoundResponse<string>($"Organizer with Id {id} was not found."));

            _fileService.DeleteFile(organizer.LogoUrl);
            organizer.LogoUrl = await _fileService.SaveFileAsync(logo, "logos");
            await _context.SaveChangesAsync();

            var responseData = new { Message = "Logo uploaded successfully." };
            return Ok(ResponseModelHelper.CreateSuccessResponse(responseData));
        }

        [HttpGet("{organizerId}/events")]
        public async Task<IActionResult> GetEvents(int organizerId)
        {
            var organizerExists = await _context.Organizers.AnyAsync(o => o.Id == organizerId);
            if (!organizerExists)
                return NotFound(ResponseModelHelper.CreateNotFoundResponse<string>($"Organizer {organizerId} not found."));

            var events = await _context.Events
                .Include(e => e.Organizer)
                .Where(e => e.OrganizerId == organizerId)
                .ToListAsync();

            var response = _mapper.Map<List<EventResponseDto>>(events);
            return Ok(ResponseModelHelper.CreateSuccessResponse(response));
        }
    }
}