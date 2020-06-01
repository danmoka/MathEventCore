using System.Threading.Tasks;
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

        public AdminController(ApplicationContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
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

            if (conferences == null)
            {
                return RedirectToAction("Error404", "Error");
            }

            return View(conferences);
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userManager.Users.ToListAsync();

            if (users == null)
            {
                return RedirectToAction("Error404", "Error");
            }

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