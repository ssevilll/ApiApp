using ApiApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace ApiApp.Data
{
    public class ApiAppDbContext : IdentityDbContext<AppUser>
    {
        public ApiAppDbContext(DbContextOptions<ApiAppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApiAppDbContext).Assembly);
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Organizer>().HasData(
                new Organizer { Id = 1, Name = "Tech Corp", Email = "contact@techcorp.com", Phone = "123456789", LogoUrl = "techcorp.png" },
                new Organizer { Id = 2, Name = "Music Live", Email = "info@musiclive.com", Phone = "987654321", LogoUrl = "musiclive.png" }
            );

            modelBuilder.Entity<Event>().HasData(
                new Event { Id = 1, Title = "Tech Conference 2024", Description = "A great tech conference.", Date = new DateTime(2024, 10, 15, 9, 0, 0), Location = "New York", OrganizerId = 1 },
                new Event { Id = 2, Title = "Rock Festival", Description = "An energetic rock festival.", Date = new DateTime(2024, 8, 20, 18, 0, 0), Location = "Los Angeles", OrganizerId = 2 }
            );

            modelBuilder.Entity<Ticket>().HasData(
                new Ticket { Id = 1, EventId = 1, Type = "General Admission", Price = 99.99m, QuantityAvailable = 500 },
                new Ticket { Id = 2, EventId = 1, Type = "VIP", Price = 199.99m, QuantityAvailable = 100 },
                new Ticket { Id = 3, EventId = 2, Type = "Standard", Price = 50.00m, QuantityAvailable = 1000 }
            );
        }

        public DbSet<Event> Events { get; set; }
        public DbSet<Organizer> Organizers { get; set; }
        public DbSet<Ticket> Tickets { get; set; }

    }
}
