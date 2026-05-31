using ApiApp.Data;
using ApiApp.DTOs.EventDtos;
using ApiApp.DTOs.OrganizerDtos;
using ApiApp.DTOs.TicketDtos;
using ApiApp.Helpers;
using ApiApp.Interfaces;
using ApiApp.Models;
using ApiApp.Services;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ApiApp.Controllers
{
    public class EventsController(
        ApiAppDbContext _context,
        IMapper _mapper,
        IFileService _fileService,
        IValidator<EventCreateDto> _createValidator,
        IValidator<EventUpdateDto> _updateValidator
        ) : BaseController
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var events = await _context.Events
                .Include(e => e.Organizer)
                .ToListAsync();
            var response = _mapper.Map<List<EventResponseDto>>(events);
            return Ok(ResponseModelHelper.CreateSuccessResponse(response));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var ev = await _context.Events
                .Include(e => e.Organizer)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (ev == null)
                return NotFound(ResponseModelHelper.CreateNotFoundResponse<string>($"Event with Id {id} was not found."));

            var response = _mapper.Map<EventResponseDto>(ev);
            return Ok(ResponseModelHelper.CreateSuccessResponse(response));
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create([FromForm] EventCreateDto dto)
        {
            var validation = await _createValidator.ValidateAsync(dto);
            if (!validation.IsValid)
            {
                var errors = validation.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(ResponseModelHelper.CreateErrorResponse<object>(errors));
            }

            var organizerExists = await _context.Organizers.AnyAsync(o => o.Id == dto.OrganizerId);
            if (!organizerExists)
                return BadRequest(ResponseModelHelper.CreateBadRequestResponse<string>($"Organizer with Id {dto.OrganizerId} does not exist."));

            var ev = _mapper.Map<Event>(dto);

            if (dto.Banner != null && dto.Banner.Length > 0)
            {
                ev.BannerImageUrl = await _fileService.SaveFileAsync(dto.Banner, "banners");
            }

            _context.Events.Add(ev);
            await _context.SaveChangesAsync();
            var response = _mapper.Map<EventResponseDto>(ev);
            return Ok(ResponseModelHelper.CreateSuccessResponse(response));
        }

        [HttpPatch("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Update(int id, EventUpdateDto dto)
        {
            var validation = await _updateValidator.ValidateAsync(dto);
            if (!validation.IsValid)
            {
                var errors = validation.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(ResponseModelHelper.CreateErrorResponse<object>(errors));
            }

            var ev = await _context.Events.FindAsync(id);
            if (ev == null)
                return NotFound(ResponseModelHelper.CreateNotFoundResponse<string>($"Event {id} not found."));

            if (dto.OrganizerId.HasValue)
            {
                var organizerExists = await _context.Organizers.AnyAsync(o => o.Id == dto.OrganizerId.Value);
                if (!organizerExists)
                    return BadRequest(ResponseModelHelper.CreateBadRequestResponse<string>($"Organizer with Id {dto.OrganizerId.Value} does not exist."));
            }

            _mapper.Map(dto, ev);
            await _context.SaveChangesAsync();
            var response = _mapper.Map<EventResponseDto>(ev);
            return Ok(ResponseModelHelper.CreateSuccessResponse(response));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null)
                return NotFound(ResponseModelHelper.CreateNotFoundResponse<string>($"Event {id} not found."));

            _fileService.DeleteFile(ev.BannerImageUrl);
            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();

            var responseData = new { Message = $"Event {id} was deleted successfully." };
            return Ok(ResponseModelHelper.CreateSuccessResponse(responseData));
        }

        [HttpPost("{id}/banner")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UploadBanner(int id, IFormFile banner)
        {
            if (banner == null || banner.Length == 0)
                return BadRequest(ResponseModelHelper.CreateBadRequestResponse<string>("No file uploaded."));

            var ev = await _context.Events.FindAsync(id);
            if (ev == null)
                return NotFound(ResponseModelHelper.CreateNotFoundResponse<string>($"Event {id} not found."));

            _fileService.DeleteFile(ev.BannerImageUrl);
            ev.BannerImageUrl = await _fileService.SaveFileAsync(banner, "banners");
            await _context.SaveChangesAsync();

            var response = _mapper.Map<EventResponseDto>(ev);
            return Ok(ResponseModelHelper.CreateSuccessResponse(response));
        }

        [HttpGet("{eventId}/tickets")]
        public async Task<IActionResult> GetTickets(int eventId)
        {
            var eventExists = await _context.Events.AnyAsync(e => e.Id == eventId);
            if (!eventExists)
                return NotFound(ResponseModelHelper.CreateNotFoundResponse<string>($"Event {eventId} not found."));

            var tickets = await _context.Tickets
                .Include(t => t.Event)
                .Where(t => t.EventId == eventId)
                .ToListAsync();

            var response = _mapper.Map<List<TicketResponseDto>>(tickets);
            return Ok(ResponseModelHelper.CreateSuccessResponse(response));
        }

        [HttpGet("{eventId}/organizer")]
        public async Task<IActionResult> GetOrganizer(int eventId)
        {
            var ev = await _context.Events
                .Include(e => e.Organizer)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (ev == null)
                return NotFound(ResponseModelHelper.CreateNotFoundResponse<string>($"Event {eventId} not found."));

            var response = _mapper.Map<OrganizerResponseDto>(ev.Organizer);
            return Ok(ResponseModelHelper.CreateSuccessResponse(response));
        }
    }
}