using ApiApp.Data;
using ApiApp.DTOs.EventDtos;
using ApiApp.DTOs.OrganizerDtos;
using ApiApp.DTOs.TicketDtos;
using ApiApp.DTOs.UserDtos;
using ApiApp.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Event.Tests.Helpers
{
    public static class DbContextFactory
    {
        public static ApiAppDbContext CreateInMemoryContext(string? dbName = null)
        {
            var options = new DbContextOptionsBuilder<ApiAppDbContext>()
                .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
                .Options;

            return new ApiAppDbContext(options);
        }

        public static async Task<ApiAppDbContext> CreateSeededContextAsync()
        {
            var context = CreateInMemoryContext();

            context.Organizers.AddRange(
                new Organizer { Id = 1, Name = "Tech Corp", Email = "tech@corp.com", Phone = "111" },
                new Organizer { Id = 2, Name = "Music Live", Email = "info@music.com", Phone = "222" }
            );

            context.Events.AddRange(
                new ApiApp.Models.Event
                {
                    Id = 1,
                    Title = "Tech Conference",
                    Date = DateTime.UtcNow.AddMonths(3),
                    Location = "New York",
                    OrganizerId = 1
                },
                new ApiApp.Models.Event
                {
                    Id = 2,
                    Title = "Rock Festival",
                    Date = DateTime.UtcNow.AddMonths(5),
                    Location = "Los Angeles",
                    OrganizerId = 2
                }
            );

            await context.SaveChangesAsync();
            return context;
        }
    }

    public static class MapperFactory
    {
        public static IMapper Create()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<OrganizerCreateDto, Organizer>();
                cfg.CreateMap<OrganizerUpdateDto, Organizer>();
                cfg.CreateMap<Organizer, OrganizerResponseDto>();

                cfg.CreateMap<EventCreateDto, ApiApp.Models.Event>();
                cfg.CreateMap<EventUpdateDto, ApiApp.Models.Event>();
                cfg.CreateMap<ApiApp.Models.Event, EventResponseDto>()
                   .ForMember(d => d.OrganizerName,
                              o => o.MapFrom(s => s.Organizer != null ? s.Organizer.Name : string.Empty));

                cfg.CreateMap<TicketCreateDto, Ticket>();
                cfg.CreateMap<TicketUpdateDto, Ticket>();
                cfg.CreateMap<Ticket, TicketResponseDto>();
            });

            config.AssertConfigurationIsValid();
            return config.CreateMapper();
        }
    }
}