﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MathEvent.Models;
using Microsoft.EntityFrameworkCore;
using MathEvent.Models.ViewModels;

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
            var performances = await _db.Performances
                .Include(p => p.Section)
                .Where(p => p.Start >= DateTime.Now)
                .OrderByDescending(p => p.Start).Take(3).ToListAsync();

            //var cards = new List<PerformanceViewModel>();

            //foreach (var performance in performances)
            //{
            //    var card = new PerformanceViewModel
            //    {
            //        Id = performance.Id,
            //        Name = performance.Name,
            //        Annotation = performance.Annotation,
            //        KeyWords = performance.KeyWords,
            //        Start = performance.Start,
            //        CreatorId = performance.CreatorId,
            //        CreatorName = performance.Creator.Name,
            //        DataPath = performance.DataPath,
            //        PosterName = performance.PosterName,
            //        Traffic = performance.Traffic,
            //        Type = performance.Type
            //    };

            //    cards.Add(card);
            //}

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
