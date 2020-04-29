using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MathEvent.Models;
using Microsoft.EntityFrameworkCore;
using MathEvent.Helpers.Email;

namespace MathEvent.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationContext _db;
        private readonly ILogger<HomeController> _logger;
        private readonly EmailSender _emailSender;

        public HomeController(ApplicationContext db, EmailConfiguration es)
        {
            _emailSender = new EmailSender(es);
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var performances = await _db.Performances
                .Where(p => p.Start >= DateTime.Now)
                .Include(p => p.Section)
                .OrderByDescending(p => p.Start).Take(3).ToListAsync();

            return View(performances);
        }

        [HttpGet]
        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public IActionResult About()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
