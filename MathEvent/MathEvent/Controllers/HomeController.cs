using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MathEvent.Models;
using Microsoft.EntityFrameworkCore;

namespace MathEvent.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationContext _db;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationContext db)
        {

            _db = db;
            //Performance performance = new Performance()
            //{
            //    Id = 0,
            //    Name = "Событие",
            //    Type = "Доклад",
            //    KeyWords = "blabla",
            //    Annotation = "blablabla",
            //    SectionId = 0,
            //    CreatorId = "adasdasd"

            //};
            //_db.Performances.Add(performance);
            //_db.SaveChanges();
        }

        public async Task<IActionResult> Index()
        {
            var performances = await _db.Performances.Include(p => p.Section)
                .Where(p => p.Start >= DateTime.Now)
                .OrderByDescending(p => p.Start).Take(3).ToListAsync();

            //var performances = _db.Performances.Include(b => b.Section);
            //performances = performances
            //    .Where(p => p.Start >= DateTime.Now)
            //    .OrderByDescending(p => p.DateStart).Take(3);

            return View(performances);
        }

        public IActionResult Privacy()
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
