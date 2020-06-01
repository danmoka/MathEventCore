using System.Threading.Tasks;
using MathEvent.Models;
using MathEvent.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MathEvent.Controllers
{
    /// <summary>
    /// Контроллер действий Администратора сайта
    /// </summary>
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

        /// <summary>
        /// Предоставляет представление со списком всех событий
        /// </summary>
        /// <returns>Представление со списком всех событий</returns>
        [HttpGet]
        public async Task<IActionResult> Performances()
        {
            var performances = await _db.Performances.ToListAsync();

            if (performances == null)
            {
                return RedirectToAction("Error404", "Error");
            }

            return View(performances);
        }

        /// <summary>
        /// Предоставляет представление со списком всех конференций
        /// </summary>
        /// <returns>Представление со списком всех конференций</returns>
        [HttpGet]
        public async Task<IActionResult> Conferences()
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

        /// <summary>
        /// Предоставляет представление со списком всех пользователей
        /// </summary>
        /// <returns>Представление со списком всех пользователей</returns>
        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();

            if (users == null)
            {
                return RedirectToAction("Error404", "Error");
            }

            return View(users);
        }

        /// <summary>
        /// Предоставляет представление для ввода сообщения
        /// </summary>
        /// <returns>Представление для ввода сообщения</returns>
        [HttpGet]
        public IActionResult Message(string userEmail)
        {
            AdminEmailViewModel model = new AdminEmailViewModel
            {
                EmailTo = userEmail
            };

            return View(model);
        }
    }
}