using Microsoft.EntityFrameworkCore;

namespace SmartCityParking.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<ParkingSlot> ParkingSlots { get; set; }
        public DbSet<Booking> Bookings { get; set; }
    }
}