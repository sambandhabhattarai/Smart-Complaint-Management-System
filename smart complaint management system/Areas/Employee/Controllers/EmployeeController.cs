using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using smart_complaint_management_system.Data;
using smart_complaint_management_system.Models;
using System.Security.Claims;

namespace smart_complaint_management_system.Areas.Employee.Controllers
{
    [Area("Employee")]
    public class EmployeeController : Controller
    {
        public readonly AppDbContext _context;

        public EmployeeController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult EmployeeRegister()
        {
            return View();
        }

        [HttpPost]
        public IActionResult EmployeeRegister(Employees employee)
        {
            if (ModelState.IsValid)
            {
                if (_context.employees.Any(e => e.EmployeeEmail == employee.EmployeeEmail))
                {
                    ModelState.AddModelError("Email", "Email is already registered.");
                    return View(employee);
                }

                _context.employees.Add(employee);
                _context.SaveChanges();
                TempData["success"] = "Employee Registered Successfully";

                return RedirectToAction("EmployeeLogin", "Employee"); 
            }

            return View(employee);
        }

        public IActionResult EmployeeLogin()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> EmployeeLogin([Bind("EmployeeEmail,Password")] Employees employee)
        {
            await HttpContext.SignOutAsync("EmployeeAuth");
            ModelState.Clear();

            var employeeExists = _context.employees.FirstOrDefault(c => c.EmployeeEmail == employee.EmployeeEmail && c.Password == employee.Password);

            if (employeeExists != null)
            {
                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, employeeExists.EmployeeId.ToString()),
                new Claim(ClaimTypes.Name, employeeExists.EmployeeName),
                new Claim(ClaimTypes.Email, employeeExists.EmployeeEmail),
                new Claim(ClaimTypes.Role, "Employee")
            };

                var identity = new ClaimsIdentity(claims, "EmployeeAuth");
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync("EmployeeAuth", principal);
                TempData["success"] = "Login successful!";
                return RedirectToAction("EmployeeDashboard");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(employee);
            }
        }

        [Authorize(AuthenticationSchemes = "EmployeeAuth")]
        public async Task<IActionResult> Employeedashboard()
        {
            var employeeName = User.Identity.Name;
            ViewBag.EmployeeName = employeeName;
            var employeeEmail = User.FindFirstValue(ClaimTypes.Email); 
            var employee = await _context.employees.FirstOrDefaultAsync(e => e.EmployeeEmail == employeeEmail);

            if (employee == null)
            {
                return Unauthorized(); 
            }
            ViewBag.IsHandlingComplaint = employee.IsHandlingComplaint;

            var complaints = await _context.complaints
                .Where(c => c.ComplaintType == employee.Category)
                .ToListAsync();

            return View(complaints);
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = "EmployeeAuth")]
        public async Task<IActionResult> TakeAction(int complaintId)
        {
            var employeeEmail = User.FindFirstValue(ClaimTypes.Email);
            var employee = await _context.employees.FirstOrDefaultAsync(e => e.EmployeeEmail == employeeEmail);

            if (employee == null)
            {
                return Unauthorized();
            }

            if (employee.IsHandlingComplaint)
            {
                TempData["error"] = "You can only handle one complaint at a time.";
                return RedirectToAction("Employeedashboard");
            }

            var complaint = await _context.complaints.FindAsync(complaintId);

            if (complaint == null || complaint.Status != "Pending")
            {
                TempData["error"] = "This complaint is no longer available.";
                return RedirectToAction("Employeedashboard");
            }

            complaint.EmployeeId = employee.EmployeeId;
            complaint.Status = "In Progress"; 
            employee.IsHandlingComplaint = true;

            await _context.SaveChangesAsync();

            TempData["success"] = "You are now handling this complaint.";
            return RedirectToAction("Employeedashboard");
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = "EmployeeAuth")]
        public IActionResult UpdateComplaintStatus(int complaintId, string status)
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["error"] = "User not authenticated. Please log in.";
                return RedirectToAction("EmployeeLogin", "Employee");
            }

            var employeeEmail = User.FindFirstValue(ClaimTypes.Email);
            Console.WriteLine($"Logged-in Employee Email: {employeeEmail}");

            var employee = _context.employees.FirstOrDefault(e => e.EmployeeEmail == employeeEmail);

            if (employee == null)
            {
                TempData["error"] = "Employee not found in the database.";
                return RedirectToAction("EmployeeDashboard");
            }

            var complaint = _context.complaints.FirstOrDefault(c => c.Id == complaintId);
            if (complaint == null)
            {
                Console.WriteLine($"Complaint with ID {complaintId} not found.");
                TempData["error"] = "Complaint not found.";
                return RedirectToAction("EmployeeDashboard");
            }

            if (complaint.Status == "In Progress" && status == "Resolved")
            {
                if (complaint.EmployeeId != employee.EmployeeId)
                {
                    TempData["error"] = "You are not authorized to resolve this complaint.";
                    return RedirectToAction("EmployeeDashboard");
                }
                employee.IsHandlingComplaint = false; 
            }

            if (status == "In Progress")
            {
                var existingComplaint = _context.complaints.FirstOrDefault(c => c.EmployeeId == employee.EmployeeId && c.Status == "In Progress");
                if (existingComplaint != null && existingComplaint.Id != complaintId)
                {
                    TempData["error"] = "You can only handle one complaint at a time.";
                    return RedirectToAction("EmployeeDashboard");
                }
                complaint.EmployeeId = employee.EmployeeId;
                employee.IsHandlingComplaint = true;
            }

            complaint.Status = status;
            _context.SaveChanges();

            TempData["success"] = "Complaint status updated successfully.";
            return RedirectToAction("EmployeeDashboard");
        }

        public async Task<IActionResult> EmployeeLogout()
        {
            await HttpContext.SignOutAsync("EmployeeAuth");
            HttpContext.Session.Clear();
            Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, max-age=0";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";
            return RedirectToAction("EmployeeLogin", "Employee", new { area = "Employee" });
        }
    }
}
