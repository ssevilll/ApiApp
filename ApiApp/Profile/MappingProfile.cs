using ApiApp.DTOs.EventDtos;
using ApiApp.DTOs.OrganizerDtos;
using ApiApp.DTOs.TicketDtos;
using ApiApp.DTOs.UserDtos;
using ApiApp.Models;

namespace ApiApp.Profile
{
    public class MappingProfile : AutoMapper.Profile
    {
        public MappingProfile()
        {

            CreateMap<OrganizerCreateDto, Organizer>();
            CreateMap<OrganizerUpdateDto, Organizer>();
            CreateMap<Organizer, OrganizerResponseDto>();


            CreateMap<EventCreateDto, Event>();
            CreateMap<EventUpdateDto, Event>();
            CreateMap<Event, EventResponseDto>();


            CreateMap<TicketCreateDto, Ticket>();
            CreateMap<TicketUpdateDto, Ticket>();
            CreateMap<Ticket, TicketResponseDto>();

            CreateMap<RegisterDto, AppUser>();
        }
    }
}
