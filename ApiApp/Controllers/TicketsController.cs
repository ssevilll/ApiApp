using ApiApp.Data;
using ApiApp.DTOs.EventDtos;
using ApiApp.DTOs.TicketDtos;
using ApiApp.Helpers;
using ApiApp.Models;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ApiApp.Controllers
{
    public class TicketsController(
        ApiAppDbContext _context,
        IMapper _mapper,
        IValidator<TicketCreateDto> _createValidator,
        IValidator<TicketUpdateDto> _updateValidator
        ) : BaseController
    {

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tickets = await _context.Tickets
                .Include(t => t.Event)
                .ToListAsync();
            var response = _mapper.Map<List<TicketResponseDto>>(tickets);
            return Ok(ResponseModelHelper.CreateSuccessResponse(response));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Event)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null)
                return NotFound(ResponseModelHelper.CreateNotFoundResponse<string>($"Ticket with Id {id} was not found."));

            var response = _mapper.Map<TicketResponseDto>(ticket);
            return Ok(ResponseModelHelper.CreateSuccessResponse(response));
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create(TicketCreateDto dto)
        {
            var validation = await _createValidator.ValidateAsync(dto);
            if (!validation.IsValid)
            {
                var error = validation.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(ResponseModelHelper.CreateBadRequestResponse<object>(error));
            }

            var eventExists = await _context.Events.AnyAsync(e => e.Id == dto.EventId);
            if (!eventExists)
                return BadRequest(ResponseModelHelper.CreateBadRequestResponse<string>($"Event with Id {dto.EventId} does not exist."));

            var ticket = _mapper.Map<Ticket>(dto);
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            var responseData = new { Message = "Ticket created successfully." };
            return Ok(ResponseModelHelper.CreateSuccessResponse(responseData));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Update(int id, TicketUpdateDto dto)
        {
            var validation = await _updateValidator.ValidateAsync(dto);
            if (!validation.IsValid)
            {
                var error = validation.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(ResponseModelHelper.CreateBadRequestResponse<object>(error));
            }

            var ticket = await _context.Tickets
                .Include(t => t.Event)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null)
                return NotFound(ResponseModelHelper.CreateNotFoundResponse<string>($"Ticket with Id {id} was not found."));

            _mapper.Map(dto, ticket);
            await _context.SaveChangesAsync();

            var responseData = new { Message = "Ticket updated successfully." };
            return Ok(ResponseModelHelper.CreateSuccessResponse(responseData));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
                return NotFound(ResponseModelHelper.CreateNotFoundResponse<string>($"Ticket with Id {id} was not found."));

            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();

            var responseData = new { Message = $"Ticket {id} was deleted successfully." };
            return Ok(ResponseModelHelper.CreateSuccessResponse(responseData));
        }
    }
}