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

        [HttpPost]
        public async Task<IActionResult> Send([Bind("EmailTo", "Content", "Message")] AdminEmailViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Проверьте введенные данные");

                return View(model);
            }

            model.Content = "Администрация MathEvent: " + model.Content;

            try
            {
                var emailMessage = new Message(new string[] { model.EmailTo }, model.Content, model.Message);
                await _emailSender.SendEmailAsync(emailMessage);
            }
            catch
            {
                return RedirectToAction("Error500", "Error");
            }

            return RedirectToAction("GetUsers", "Admin");
        }
    }
}