using ApiApp.Data;
using ApiApp.DTOs.EventDtos;
using ApiApp.DTOs.OrganizerDtos;
using ApiApp.DTOs.TicketDtos;
using ApiApp.Interfaces;
using ApiApp.Models;
using ApiApp.Services;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiApp.Controllers
{
    [ApiController]
    [Route("api/events")]
    public class EventsController(
        ApiAppDbContext _context,
        IMapper _mapper,
        IFileService _fileService,
        IValidator<EventCreateDto> _createValidator,
        IValidator<EventUpdateDto> _updateValidator
        ) : ControllerBase
    {


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var events = await _context.Events
                .Include(e => e.Organizer)
                .ToListAsync();
            return Ok(_mapper.Map<List<EventResponseDto>>(events));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var ev = await _context.Events
                .Include(e => e.Organizer)
                .FirstOrDefaultAsync(e => e.Id == id);
            if (ev == null) return NotFound();
            return Ok(_mapper.Map<EventResponseDto>(ev));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] EventCreateDto dto)
        {
            var validation = await _createValidator.ValidateAsync(dto);
            if (!validation.IsValid)
                return BadRequest(validation.Errors.Select(e => e.ErrorMessage));

            var organizerExists = await _context.Organizers.AnyAsync(o => o.Id == dto.OrganizerId);
            if (!organizerExists)
                return BadRequest($"Organizer with Id {dto.OrganizerId} does not exist.");

            var ev = _mapper.Map<Event>(dto);

            if (dto.Banner != null && dto.Banner.Length > 0)
            {
                ev.BannerImageUrl = await _fileService.SaveFileAsync(dto.Banner, "banners");
            }

            _context.Events.Add(ev);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, EventUpdateDto dto)
        {
            var validation = await _updateValidator.ValidateAsync(dto);
            if (!validation.IsValid)
                return BadRequest(validation.Errors.Select(e => e.ErrorMessage));

            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();

            var organizerExists = await _context.Organizers.AnyAsync(o => o.Id == dto.OrganizerId);
            if (!organizerExists)
                return BadRequest($"Organizer with Id {dto.OrganizerId} does not exist.");

            _mapper.Map(dto, ev);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();

            _fileService.DeleteFile(ev.BannerImageUrl);
            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("{id}/banner")]
        public async Task<IActionResult> UploadBanner(int id, IFormFile banner)
        {
            if (banner == null || banner.Length == 0)
                return BadRequest("No file uploaded.");

            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();

            _fileService.DeleteFile(ev.BannerImageUrl);
            ev.BannerImageUrl = await _fileService.SaveFileAsync(banner, "banners");
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("{eventId}/tickets")]
        public async Task<IActionResult> GetTickets(int eventId)
        {
            var eventExists = await _context.Events.AnyAsync(e => e.Id == eventId);
            if (!eventExists) return NotFound($"Event {eventId} not found.");

            var tickets = await _context.Tickets
                .Include(t => t.Event)
                .Where(t => t.EventId == eventId)
                .ToListAsync();

            return Ok();
        }


        [HttpGet("{eventId}/organizer")]
        public async Task<IActionResult> GetOrganizer(int eventId)
        {
            var ev = await _context.Events
                .Include(e => e.Organizer)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (ev == null) return NotFound($"Event {eventId} not found.");
            return Ok();
        }
    }
}
