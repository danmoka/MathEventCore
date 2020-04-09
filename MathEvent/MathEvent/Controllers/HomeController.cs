using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MathEvent.Models;

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

        public IActionResult Index()
        {
            return View();
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
