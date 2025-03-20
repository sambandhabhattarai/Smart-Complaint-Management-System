using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using smart_complaint_management_system.Data;
using smart_complaint_management_system.Models;

namespace smart_complaint_management_system.Areas.Admin.Controllers
{
    /*[RestrictIP]*/

    [Area("Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult adminDashboard()
        {
            var complaints = _context.complaints
                .Include(c => c.User)
                .Select(c => new Complaints
                {
                    Id = c.Id,
                    Location = c.Location,
                    ComplaintType = c.ComplaintType,
                    Status = c.Status,
                    Latitude = c.Latitude,
                    Longitude = c.Longitude,
                    User = c.User
                })
                .ToList();
            var employees = _context.employees.ToList();
            ViewBag.Employees = employees;
            return View(complaints);
        }

        [HttpPost]
        public IActionResult Update(int id, string status)
        {
            var complaint = _context.complaints.Find(id);

            if (complaint == null)
            {
                return NotFound();
            }

            complaint.Status = status;
            _context.SaveChanges();

            TempData["success"] = "Complaint status updated successfully!";
            return RedirectToAction("adminDashboard");
        }
    }
}