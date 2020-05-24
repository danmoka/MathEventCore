using System.Threading.Tasks;
using MathEvent.Helpers.Email;
using MathEvent.Models;
using MathEvent.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MathEvent.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly EmailSender _emailSender;

        public AdminController(ApplicationContext db, UserManager<ApplicationUser> userManager, EmailConfiguration er)
        {
            _db = db;
            _userManager = userManager;
            _emailSender = new EmailSender(er);
        }

        [HttpGet]
        public async Task<IActionResult> GetPerformances()
        {
            var performances = await _db.Performances.ToListAsync();

            if (performances == null)
            {
                return RedirectToAction("Error404", "Error");
            }

            return View(performances);
        }

        [HttpGet]
        public async Task<IActionResult> GetConferences()
        {
            var conferences = await _db.Conferences
                .Include(c => c.Sections)
                .ThenInclude(s => s.Performances)
                .ToListAsync();

            return View(conferences);
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userManager.Users.ToListAsync();

            return View(users);
        }

        [HttpGet]
        public IActionResult Send(string userEmail)
        {
            AdminEmailViewModel model = new AdminEmailViewModel
            {
                EmailTo = userEmail
            };

            return View(model);
        }
    }
}