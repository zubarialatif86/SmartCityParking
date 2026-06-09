using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SmartCityParking.Models;
using System.Linq;

namespace SmartCityParking.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // User Login
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.Password == password);
            if (user != null)
            {
                HttpContext.Session.SetString("UserEmail", email);
                HttpContext.Session.SetString("UserName", user.FullName);
                return RedirectToAction("Index", "Dashboard");
            }
            ViewBag.Error = "Invalid credentials";
            return View();
        }

        // User Register
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public IActionResult Register(string fullName, string phone, string email, string password, string confirmPassword)
        {
            if (password != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match";
                return View();
            }
            if (_context.Users.Any(u => u.Email == email))
            {
                ViewBag.Error = "Email already registered";
                return View();
            }
            var user = new User
            {
                FullName = fullName,
                Phone = phone,
                Email = email,
                Password = password,
                WalletBalance = 500
            };
            _context.Users.Add(user);
            _context.SaveChanges();
            return RedirectToAction("Login");
        }

        // Admin Login
        [HttpGet]
        public IActionResult AdminLogin()
        {
            if (HttpContext.Session.GetString("AdminLoggedIn") == "true")
                return RedirectToAction("Dashboard", "Admin");
            return View();
        }

        [HttpPost]
        public IActionResult AdminLogin(string username, string password)
        {
            if (username == "admin" && password == "admin123")
            {
                HttpContext.Session.SetString("AdminLoggedIn", "true");
                return RedirectToAction("Dashboard", "Admin");
            }
            ViewBag.Error = "Invalid admin credentials";
            return View();
        }

        public IActionResult AdminLogout()
        {
            HttpContext.Session.Remove("AdminLoggedIn");
            return RedirectToAction("Index", "Home");
        }
    }
}