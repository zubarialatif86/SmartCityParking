using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCityParking.Models;
using System.Text;
using System.Text.Json;

namespace SmartCityParking.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Check if admin is logged in
        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("AdminLoggedIn") == "true";
        }

        // ========================= DASHBOARD =========================
        public IActionResult Dashboard()
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Account");

            var today = DateTime.Today;
            var totalSlots = _context.ParkingSlots.Count();
            var availableSlots = _context.ParkingSlots.Count(s => s.IsAvailable);
            var totalUsers = _context.Users.Count();
            var activeBookings = _context.Bookings.Count(b => b.Status == "Active");
            var totalRevenue = _context.Bookings.Where(b => b.Status == "Completed").Sum(b => b.TotalAmount);
            var todayBookings = _context.Bookings.Count(b => b.StartTime.Date == today);
            var todayRevenue = _context.Bookings.Where(b => b.StartTime.Date == today && b.Status == "Completed").Sum(b => b.TotalAmount);
            var occupancyRate = totalSlots == 0 ? 0 : (totalSlots - availableSlots) * 100 / totalSlots;

            ViewBag.TotalSlots = totalSlots;
            ViewBag.AvailableSlots = availableSlots;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.ActiveBookings = activeBookings;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TodayBookings = todayBookings;
            ViewBag.TodayRevenue = todayRevenue;
            ViewBag.OccupancyRate = occupancyRate;

            // Last 7 days data for charts
            var dates = Enumerable.Range(0, 7).Select(i => today.AddDays(-i).Date).Reverse().ToList();
            var labels = dates.Select(d => d.ToString("dd/MM")).ToList();
            var bookingsData = dates.Select(d => _context.Bookings.Count(b => b.StartTime.Date == d)).ToList();
            var revenueData = dates.Select(d => _context.Bookings.Where(b => b.StartTime.Date == d && b.Status == "Completed").Sum(b => b.TotalAmount)).ToList();

            ViewBag.Labels = JsonSerializer.Serialize(labels);
            ViewBag.BookingsData = JsonSerializer.Serialize(bookingsData);
            ViewBag.RevenueData = JsonSerializer.Serialize(revenueData);

            // Recent 5 bookings
            ViewBag.RecentBookings = _context.Bookings
                .Include(b => b.ParkingSlot)
                .OrderByDescending(b => b.StartTime)
                .Take(5)
                .ToList();

            // Top 3 most used slots
            var topSlots = _context.Bookings
                .GroupBy(b => b.ParkingSlotId)
                .Select(g => new { SlotId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(3)
                .Join(_context.ParkingSlots, x => x.SlotId, s => s.Id, (x, s) => new { s.SlotNumber, x.Count })
                .ToList();
            ViewBag.TopSlots = topSlots;

            return View();
        }

        // ========================= PARKING SLOTS =========================
        public async Task<IActionResult> ParkingSlots()
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Account");
            return View(await _context.ParkingSlots.ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> AddSlot(ParkingSlot slot)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Account");

            if (ModelState.IsValid)
            {
                _context.ParkingSlots.Add(slot);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Slot added successfully!";
            }
            else
            {
                TempData["Error"] = "Invalid data. Please check all fields.";
            }
            return RedirectToAction(nameof(ParkingSlots));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSlot(ParkingSlot slot)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Account");

            var existing = await _context.ParkingSlots.FindAsync(slot.Id);
            if (existing != null)
            {
                existing.SlotNumber = slot.SlotNumber;
                existing.Location = slot.Location;
                existing.VehicleType = slot.VehicleType;
                existing.PricePerHour = slot.PricePerHour;
                existing.IsAvailable = slot.IsAvailable;
                await _context.SaveChangesAsync();
                TempData["Message"] = "Slot updated successfully!";
            }
            else
            {
                TempData["Error"] = "Slot not found.";
            }
            return RedirectToAction(nameof(ParkingSlots));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSlot(int id)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Account");

            var slot = await _context.ParkingSlots.FindAsync(id);
            if (slot != null)
            {
                _context.ParkingSlots.Remove(slot);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Slot deleted!";
            }
            else
            {
                TempData["Error"] = "Slot not found.";
            }
            return RedirectToAction(nameof(ParkingSlots));
        }

        // ========================= BOOKINGS =========================
        public async Task<IActionResult> Bookings(string status = "All", string search = "")
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Account");

            var query = _context.Bookings.Include(b => b.ParkingSlot).AsQueryable();
            if (status != "All") query = query.Where(b => b.Status == status);
            if (!string.IsNullOrEmpty(search)) query = query.Where(b => b.VehicleNumber.Contains(search));

            ViewBag.CurrentStatus = status;
            ViewBag.Search = search;
            return View(await query.OrderByDescending(b => b.StartTime).ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> CancelBooking(int id)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Account");

            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null && booking.Status == "Active")
            {
                booking.Status = "Cancelled";
                // Free the slot
                var slot = await _context.ParkingSlots.FindAsync(booking.ParkingSlotId);
                if (slot != null) slot.IsAvailable = true;
                await _context.SaveChangesAsync();
                TempData["Message"] = "Booking cancelled and slot freed.";
            }
            else
            {
                TempData["Error"] = "Cannot cancel this booking.";
            }
            return RedirectToAction(nameof(Bookings));
        }

        // ========================= USER MANAGEMENT =========================
        public async Task<IActionResult> Users()
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Account");
            return View(await _context.Users.ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> ToggleBlockUser(int id)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Account");

            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                user.IsBlocked = !user.IsBlocked;
                await _context.SaveChangesAsync();
                TempData["Message"] = $"User {(user.IsBlocked ? "blocked" : "unblocked")}.";
            }
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Account");

            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                TempData["Message"] = "User deleted.";
            }
            return RedirectToAction(nameof(Users));
        }

        // ========================= REPORTS =========================
        public IActionResult Reports()
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Account");

            var currentYear = DateTime.Now.Year;
            var months = Enumerable.Range(1, 12);
            var monthlyRevenue = months
                .Select(m => _context.Bookings
                    .Where(b => b.StartTime.Year == currentYear && b.StartTime.Month == m && b.Status == "Completed")
                    .Sum(b => b.TotalAmount))
                .ToList();

            ViewBag.Months = JsonSerializer.Serialize(months.Select(m => new DateTime(currentYear, m, 1).ToString("MMM")));
            ViewBag.MonthlyRevenue = JsonSerializer.Serialize(monthlyRevenue);

            // Slot utilization (top 10)
            var utilization = _context.ParkingSlots
                .Select(s => new
                {
                    s.SlotNumber,
                    BookingCount = _context.Bookings.Count(b => b.ParkingSlotId == s.Id)
                })
                .OrderByDescending(x => x.BookingCount)
                .Take(10)
                .ToList();

            ViewBag.UtilizationLabels = JsonSerializer.Serialize(utilization.Select(x => x.SlotNumber));
            ViewBag.UtilizationData = JsonSerializer.Serialize(utilization.Select(x => x.BookingCount));

            return View();
        }

        public IActionResult ExportBookingsCSV()
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Account");

            var bookings = _context.Bookings.Include(b => b.ParkingSlot).ToList();
            var csv = new StringBuilder();
            csv.AppendLine("ID,VehicleNumber,Slot,StartTime,EndTime,Amount,Status");
            foreach (var b in bookings)
            {
                csv.AppendLine($"{b.Id},{b.VehicleNumber},{b.ParkingSlot?.SlotNumber},{b.StartTime},{b.EndTime},{b.TotalAmount},{b.Status}");
            }
            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"Bookings_{DateTime.Now:yyyyMMdd}.csv");
        }
        // GET: Add Slot page
        public IActionResult AddSlot()
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Account");
            return View();
        }

        // GET: List of slots to choose for editing
        public async Task<IActionResult> EditSlotList()
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Account");
            var slots = await _context.ParkingSlots.ToListAsync();
            return View(slots);
        }

        // GET: Edit specific slot
        public async Task<IActionResult> EditSlot(int id)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Account");
            var slot = await _context.ParkingSlots.FindAsync(id);
            if (slot == null) return NotFound();
            return View(slot);
        }

        // ========================= SETTINGS (optional) =========================
        public IActionResult Settings()
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Account");
            return View();
        }

        [HttpPost]
        public IActionResult UpdateSettings(string siteName, decimal defaultRate, int bookingTimeout)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Account");
            // For demo, just store in session
            HttpContext.Session.SetString("SiteName", siteName);
            HttpContext.Session.SetString("DefaultRate", defaultRate.ToString());
            TempData["Message"] = "Settings updated successfully!";
            return RedirectToAction(nameof(Settings));
        }
    }
}