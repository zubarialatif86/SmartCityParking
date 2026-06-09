using Microsoft.AspNetCore.Mvc;
using SmartCityParking.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace SmartCityParking.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Yahan hum current user ki bookings nikalenge
            // Note: Agar aapne login ke liye Session use kiya hai, toh email wahan se lein
            var userEmail = HttpContext.Session.GetString("UserEmail") ?? "Guest_Node@127.0.0.1";

            var myBookings = _context.Bookings
                .Include(b => b.ParkingSlot)
                .Where(b => b.UserEmail == userEmail)
                .OrderByDescending(b => b.StartTime)
                .ToList();

            return View(myBookings);
        }
    }
}