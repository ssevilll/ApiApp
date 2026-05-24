using ApiApp.Data;
using ApiApp.DTOs.EventDtos;
using ApiApp.DTOs.TicketDtos;
using ApiApp.Models;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ApiApp.Controllers
{
    [ApiController]
    [Route("api/tickets")]
    public class TicketsController(
        ApiAppDbContext _context,
        IMapper _mapper,
        IValidator<TicketCreateDto> _createValidator,
        IValidator<TicketUpdateDto> _updateValidator
        ) : ControllerBase
    {

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tickets = await _context.Tickets
            .Include(t => t.Event)
                .ToListAsync();
            return Ok(_mapper.Map<List<TicketResponseDto>>(tickets));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Event)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (ticket == null) return NotFound();
            return Ok(_mapper.Map<List<TicketResponseDto>>(ticket));
        }

        [HttpPost]
        public async Task<IActionResult> Create(TicketCreateDto dto)
        {
            var validation = await _createValidator.ValidateAsync(dto);
            if (!validation.IsValid)
                return BadRequest(validation.Errors.Select(e => e.ErrorMessage));

            var eventExists = await _context.Events.AnyAsync(e => e.Id == dto.EventId);
            if (!eventExists)
                return BadRequest($"Event with Id {dto.EventId} does not exist.");

            var ticket = _mapper.Map<Ticket>(dto);
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, TicketUpdateDto dto)
        {
            var validation = await _updateValidator.ValidateAsync(dto);
            if (!validation.IsValid)
                return BadRequest(validation.Errors.Select(e => e.ErrorMessage));

            var ticket = await _context.Tickets
                .Include(t => t.Event)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (ticket == null) return NotFound();

            _mapper.Map(dto, ticket);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null) return NotFound();

            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
