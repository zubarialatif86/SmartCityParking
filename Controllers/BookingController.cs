using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCityParking.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using QRCoder;
using Microsoft.AspNetCore.Http;

namespace SmartCityParking.Controllers
{
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Search(string searchString)
        {
            var slotsQuery = _context.ParkingSlots.AsQueryable();
            if (!string.IsNullOrEmpty(searchString))
            {
                // Use existing ParkingSlot properties: Location and SlotNumber
                slotsQuery = slotsQuery.Where(s => s.Location.Contains(searchString) || s.SlotNumber.Contains(searchString));
            }

            var activeBookings = await _context.Bookings
                .Include(b => b.ParkingSlot)
                .OrderByDescending(b => b.StartTime)
                .ToListAsync();

            ViewBag.ActiveBookings = activeBookings;
            ViewData["CurrentFilter"] = searchString;
            return View(await slotsQuery.ToListAsync());
        }

        // --- BOOKING METHOD (FIXED) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookSlot(int slotId, int hours = 1)
        {
            // Session se email uthayein (Jo Login karte waqt save kiya tha)
            var activeUserEmail = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(activeUserEmail))
            {
                // Agar user login nahi hai toh login page par bhejen
                return RedirectToAction("Login", "Account");
            }

            var slot = await _context.ParkingSlots.FindAsync(slotId);
            // Use IsAvailable (existing) instead of IsReserved
            if (slot == null || !slot.IsAvailable) return RedirectToAction(nameof(Search));

            var bookingRecord = new Booking
            {
                ParkingSlotId = slot.Id,
                UserEmail = activeUserEmail, // Ab sahi email save hoga
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(hours),
                TotalAmount = slot.PricePerHour * hours,
                BookingStatus = "Confirmed"
            };

            // Mark slot as no longer available
            slot.IsAvailable = false;
            _context.Bookings.Add(bookingRecord);
            await _context.SaveChangesAsync();

            // QR Code Logic...
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            using (QRCodeData qrCodeData = qrGenerator.CreateQrCode($"BookingID: {bookingRecord.Id}, Slot: {slot.SlotNumber}", QRCodeGenerator.ECCLevel.Q))
            using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
            {
                byte[] qrCodeImage = qrCode.GetGraphic(20);
                string base64QR = "data:image/png;base64," + Convert.ToBase64String(qrCodeImage);
                HttpContext.Session?.SetString("LastQR", base64QR);
            }

            return RedirectToAction("Index", "Dashboard", new { controller = "Dashboard" });
        }

        // --- BAAQI METHODS (Edit, Create, Release) ---
        public async Task<IActionResult> Edit(int id)
        {
            var slot = await _context.ParkingSlots.FindAsync(id);
            if (slot == null) return NotFound();
            return View(slot);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ParkingSlot slot)
        {
            if (id != slot.Id) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(slot);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Search));
            }
            return View(slot);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ParkingSlot slot)
        {
            if (ModelState.IsValid)
            {
                _context.ParkingSlots.Add(slot);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Search));
            }
            return View(slot);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Release(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();
            var slot = await _context.ParkingSlots.FindAsync(booking.ParkingSlotId);
            if (slot != null) slot.IsAvailable = true; // Release sets availability
            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Dashboard");
        }
    }
}