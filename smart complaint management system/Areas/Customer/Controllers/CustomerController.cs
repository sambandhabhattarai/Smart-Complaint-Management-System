using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using smart_complaint_management_system.Data;
using smart_complaint_management_system.Models;
using System.Security.Claims;

namespace smart_complaint_management_system.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CustomerController : Controller
    {
        private readonly AppDbContext _context;

        public CustomerController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(User user)
        {
            if (!ModelState.IsValid)
            {
                return View(user);
            }

            bool userExists = _context.users
                .Any(c => c.Name.Trim().ToLower() == user.Name.Trim().ToLower() ||
                          c.Email.Trim().ToLower() == user.Email.Trim().ToLower());

            if (userExists)
            {
                ModelState.AddModelError("Name", "User with this name or email already exists.");
                return View(user);
            }

            _context.users.Add(user);
            _context.SaveChanges();
            TempData["success"] = "User added successfully";
            return RedirectToAction("Login");
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login([Bind("Email,Password")] User user)
        {
            await HttpContext.SignOutAsync("CustomerAuth");
            ModelState.Clear();
            var userExists = _context.users.FirstOrDefault(c => c.Email == user.Email && c.Password == user.Password);

            if (userExists != null)
            {
                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userExists.Id.ToString()),
                new Claim(ClaimTypes.Name, userExists.Name),
                new Claim(ClaimTypes.Email, userExists.Email),
                new Claim(ClaimTypes.Role, "Customer")
            };

                var identity = new ClaimsIdentity(claims, "CustomerAuth");
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync("CustomerAuth", principal);
                TempData["Success"] = "Login successful!";
                return RedirectToAction("Dashboard");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(user);
            }
        }

        [Authorize]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None, Duration = 0)]
        public IActionResult Dashboard()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Customer"); 
            }
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)); 

            var userComplaints = _context.complaints
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new
                {
                    c.Id,
                    c.Location,
                    c.ComplaintType,
                    c.CreatedAt,
                    c.Status,
                    c.AssignedEmployee
                })
                .ToList();
            return View(userComplaints);
        }

        [Authorize]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None, Duration = 0)]
        public IActionResult Complaint()
        {
            HttpContext.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, max-age=0";
            HttpContext.Response.Headers["Pragma"] = "no-cache";
            HttpContext.Response.Headers["Expires"] = "0";

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Complaint(Complaints complaint, IFormFile ComplaintImage)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
            {
                return Unauthorized(); 
            }

            var user = await _context.users.FindAsync(userId);
            if (user == null)
            {
                return BadRequest("User not found.");
            }

            if (ComplaintImage != null && ComplaintImage.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await ComplaintImage.CopyToAsync(memoryStream);
                    complaint.Photo = memoryStream.ToArray();
                }
            }

            complaint.UserId = user.Id;
            complaint.CreatedAt = DateTime.Now;

            _context.complaints.Add(complaint);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Complaint Filed Successfully!";
            return RedirectToAction("Dashboard");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CustomerAuth");
            HttpContext.Session.Clear();
            Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, max-age=0";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";
            return RedirectToAction("Login", "Customer", new { area = "Customer" });
        }

        [Authorize]
        [HttpGet]
        public IActionResult Profile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(); 
            }

            var user = _context.users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            return View(user); 
        }

        [Authorize]
        public IActionResult Edit()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var user = _context.users.FirstOrDefault(c => c.Id == userId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(User model, IFormFile? ProfilePhoto) 
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.users.FindAsync(model.Id);
            if (user == null)
            {
                return NotFound("User not found");
            }

            if (!string.IsNullOrEmpty(model.Name))
            {
                user.Name = model.Name;
            }

            if (!string.IsNullOrEmpty(model.Email))
            {
                user.Email = model.Email;
            }

            if (!string.IsNullOrEmpty(model.Phone))
            {
                user.Phone = model.Phone;  
            }

            if (!string.IsNullOrEmpty(model.Password))
            {
                user.Password = model.Password;
            }

            if (ProfilePhoto != null && ProfilePhoto.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await ProfilePhoto.CopyToAsync(memoryStream);
                    user.Photo = memoryStream.ToArray();
                }
            }

            _context.users.Update(user);
            await _context.SaveChangesAsync();

            TempData["success"] = "Profile updated successfully!";
            return RedirectToAction("Profile");
        }

    }
}
