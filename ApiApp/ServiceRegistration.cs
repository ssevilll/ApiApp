using ApiApp.Data;
using ApiApp.DTOs.EventDtos;
using ApiApp.DTOs.OrganizerDtos;
using ApiApp.DTOs.TicketDtos;
using ApiApp.Interfaces;
using ApiApp.Models;
using ApiApp.Profile;
using ApiApp.Services;
using ApiApp.Validation.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

namespace ApiApp
{
    public static class ServiceRegistration
    {
        public static void AddServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();

            services.AddSwaggerGen();

            services.AddAutoMapper(typeof(MappingProfile));

            // FluentValidation
            services.AddScoped<IValidator<EventCreateDto>, EventCreateValidator>();
            services.AddScoped<IValidator<EventUpdateDto>, EventUpdateValidator>();
            services.AddScoped<IValidator<OrganizerCreateDto>, OrganizerCreateValidator>();
            services.AddScoped<IValidator<OrganizerUpdateDto>, OrganizerUpdateValidator>();
            services.AddScoped<IValidator<TicketCreateDto>, TicketCreateValidator>();
            services.AddScoped<IValidator<TicketUpdateDto>, TicketUpdateValidator>();

            // File service
            services.AddHttpContextAccessor();
            services.AddScoped<IFileService, FileService>();

            services.AddDbContext<ApiAppDbContext>(options =>
                options.UseSqlServer(config.GetConnectionString("DefaultConnection")));
            services.AddIdentity<AppUser, IdentityRole>(opt=>
            {
                    opt.Password.RequireDigit = true;
                    opt.Password.RequireLowercase = true;
                    opt.Password.RequireUppercase = true;
                    opt.Password.RequireNonAlphanumeric = false;
                    opt.Password.RequiredLength = 6;
            })
                .AddEntityFrameworkStores<ApiAppDbContext>();
        }
    }
}
