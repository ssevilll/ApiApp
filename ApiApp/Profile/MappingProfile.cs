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
            // Event mappings
            CreateMap<EventCreateDto, Event>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.BannerImageUrl, opt => opt.Ignore())
                .ForMember(dest => dest.Organizer, opt => opt.Ignore())
                .ForMember(dest => dest.Tickets, opt => opt.Ignore());
            CreateMap<EventUpdateDto, Event>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.BannerImageUrl, opt => opt.Ignore())
                .ForMember(dest => dest.Organizer, opt => opt.Ignore())
                .ForMember(dest => dest.Tickets, opt => opt.Ignore());
            CreateMap<Event, EventResponseDto>()
                .ForMember(dest=>dest.BannerImageUrl, opt=> opt.MapFrom(src => src.BannerImageUrl))
                .ForMember(dest => dest.OrganizerName,
                           opt => opt.MapFrom(src => src.Organizer != null ? src.Organizer.Name : string.Empty));

            // Organizer mappings
            CreateMap<OrganizerCreateDto, Organizer>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.LogoUrl, opt => opt.Ignore())
                .ForMember(dest => dest.Events, opt => opt.Ignore());
            CreateMap<OrganizerUpdateDto, Organizer>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.LogoUrl, opt => opt.Ignore())
                .ForMember(dest => dest.Events, opt => opt.Ignore());
            CreateMap<Organizer, OrganizerResponseDto>();

            // Ticket mappings
            CreateMap<TicketCreateDto, Ticket>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Event, opt => opt.Ignore());
            CreateMap<TicketUpdateDto, Ticket>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.EventId, opt => opt.Ignore())
                .ForMember(dest => dest.Event, opt => opt.Ignore());
            CreateMap<Ticket, TicketResponseDto>()
                .ForMember(dest => dest.EventTitle,
                           opt => opt.MapFrom(src => src.Event != null ? src.Event.Title : string.Empty));

            // AppUser mappings 
            CreateMap<RegisterDto, AppUser>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.NormalizedUserName, opt => opt.MapFrom(src => src.Email.ToUpper()))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.NormalizedEmail, opt => opt.MapFrom(src => src.Email.ToUpper()))
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.SecurityStamp, opt => opt.Ignore())
                .ForMember(dest => dest.ConcurrencyStamp, opt => opt.Ignore());
        }
    }
}
