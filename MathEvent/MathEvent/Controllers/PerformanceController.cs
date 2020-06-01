using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MathEvent.Helpers;
using MathEvent.Helpers.Access;
using MathEvent.Models;
using MathEvent.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MathEvent.Controllers
{
    public class PerformanceController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationContext _db;
        private readonly UserService _userService;

        public PerformanceController(ApplicationContext db, 
            UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager,
            UserService userService)
        {
            _db = db;
            _userManager = userManager;
            _signInManager = signInManager;
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var performances = await _db.Performances
                .Where(p => p.Start.Month >= DateTime.Now.Month)
                .Include(p => p.Section)
                .Include(p => p.Creator).ToListAsync();

            if (performances == null)
            {
                return RedirectToAction("Error404", "Error");
            }

            var cards = new List<PerformanceViewModel>();

            foreach (var performance in performances)
            {
                var card = new PerformanceViewModel
                {
                    Id = performance.Id,
                    Name = performance.Name,
                    Annotation = performance.Annotation,
                    KeyWords = performance.KeyWords,
                    Start = performance.Start,
                    DataPath = performance.DataPath,
                    PosterName = performance.PosterName,
                    Traffic = performance.Traffic,
                    Type = performance.Type,
                    Location = performance.Location,
                    IsSectionData = performance.IsSectionData
                };

                if (performance.Creator != null)
                {
                    card.CreatorName = $"{performance.Creator.Name} {performance.Creator.Surname}";
                }

                cards.Add(card);
            }

            return View(cards);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Add()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Error404", "Error");
            }

            var performance = new PerformanceViewModel
            {
                UserId = user.Id
            };

            return View(performance);
        }

        [HttpGet]
        public async Task<IActionResult> Card(int id)
        {
            var performance = await _db.Performances.Where(p => p.Id == id)
                .Include(p => p.Section)
                .Include(p => p.Creator).SingleOrDefaultAsync();

            if (performance == null)
            {
                return RedirectToAction("Error404", "Error");
            }

            var card = new PerformanceViewModel
            {
                Id = performance.Id,
                Name = performance.Name,
                Annotation = performance.Annotation,
                KeyWords = performance.KeyWords,
                Start = performance.Start,
                DataPath = performance.DataPath,
                PosterName = performance.PosterName,
                Traffic = performance.Traffic,
                Location = performance.Location,
                Type = performance.Type,
                SectionId = performance.SectionId,
                IsSectionData = performance.IsSectionData
            };

            if (performance.Creator != null)
            {
                card.CreatorName = $"{performance.Creator.Name} {performance.Creator.Surname}";
                card.UserInfo = performance.Creator.Info;
            }

            if (performance.SectionId != null)
            {
                var section = await _db.Sections.Where(s => s.Id == performance.SectionId)
                    .Include(s => s.Manager)
                    .Include(s => s.Conference)
                    .SingleOrDefaultAsync();

                if (section != null)
                {
                    var sectionInfo = $"Событие находится на секции \"{section.Name}\" ({section.Start:dd/MM/yyyy HH:mm} - {section.End:dd/MM/yyyy HH:mm}). ";

                    if (section.Manager != null)
                    {
                        sectionInfo += $"Создатель секции - {section.Manager.Name} {section.Manager.Surname}. ";
                    }

                    if (section.Conference != null)
                    {
                        sectionInfo += $"Конференция \"{section.Conference.Name}\".";
                    }

                    card.SectionInfo = sectionInfo;
                }
            }

            if (_signInManager.IsSignedIn(User))
            {
                var user = await _userManager.GetUserAsync(User);
                var userId = user.Id;

                var ap = await _db.ApplicationUserPerformances.Where(ap => ap.PerformanceId == id && ap.ApplicationUserId == userId).SingleOrDefaultAsync();

                card.IsSubscribed = ap != null;
                card.UserId = user.Id;
            }

            return View(card);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Edit(int performanceId)
        {
            var performance = await _db.Performances.Where(c => c.Id == performanceId).SingleOrDefaultAsync();

            if (performance == null)
            {
                return RedirectToAction("Error404", "Error");
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Error404", "Error");
            }

            if (! await _userService.IsPerformanceModifier(performanceId, user.Id))
            {
                return RedirectToAction("Error403", "Error");
            }

            var performanceModel = new PerformanceViewModel
            {
                Id = performance.Id,
                Type = performance.Type,
                Name = performance.Name,
                Annotation = performance.Annotation,
                KeyWords = performance.KeyWords,
                Start = performance.Start,
                UserId = user.Id,
                Location = performance.Location,
                SectionId = performance.SectionId,
                DataPath = performance.DataPath,
                IsSectionData = performance.IsSectionData
            };

            var userRoles = (List<string>)await _userManager.GetRolesAsync(user);

            if (userRoles == null)
            {
                return RedirectToAction("Error404", "Error");
            }

            return View(performanceModel);
        }

        [HttpGet]
        public IActionResult About()
        {
            return View();
        }

        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> CheckStartDate(DateTime start, int? sectionId)
        {
            if (start < DateTime.Now)
            {
                return Json($"Дата {start} меньше текущей даты {DateTime.Now}");
            }
            if (sectionId != null)
            {
                var section = await _db.Sections.Where(s => s.Id == sectionId).SingleAsync();
                if (start < section.Start || start > section.End)
                {
                    return Json($"Дата {start} выходит за временные рамки секции {section.Name}.");
                }
            }

            return Json(true);
        }

        [AcceptVerbs("Get", "Post")]
        public IActionResult CheckType(string type)
        {
            if (!DataFactory.GetPerformanceTypes().GetValues().Contains(type))
            {
                return Json($"Тип {type} не существует");
            }

            return Json(true);
        }

        [AcceptVerbs("Get", "Post")]
        public IActionResult CheckLocation(string location, bool isSectionStartAndLocation)
        {
            if (!isSectionStartAndLocation && String.IsNullOrEmpty(location))
            {
                return Json($"Выберите адрес");
            }

            return Json(true);
        }

        [AcceptVerbs("Get", "Post")]
        public IActionResult CheckStart(DateTime start, bool isSectionStartAndLocation)
        {
            if (!isSectionStartAndLocation && start == null)
            {
                return Json($"Выберите время начала");
            }

            return Json(true);
        }

        [AcceptVerbs("Get", "Post")]
        public IActionResult CheckSection(int? sectionId, bool isSectionStartAndLocation)
        {
            if (isSectionStartAndLocation && sectionId == null)
            {
                return Json($"Выберите секцию");
            }

            return Json(true);
        }

        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> CheckStartAndLocation(bool isSectionStartAndLocation, DateTime start, string location, int? sectionId)
        {
            if (!isSectionStartAndLocation)
            {
                if (start == null)
                {
                    return Json($"Выберите время начала");
                }

                if (String.IsNullOrEmpty(location))
                {
                    return Json($"Веберите адрес");
                }
            }
            else
            {
                if (sectionId == null)
                {
                    return Json($"Выберите секцию");
                }

                var section = await _db.Sections.Where(s => s.Id == sectionId).SingleOrDefaultAsync();

                if (section == null)
                {
                    return Json($"Не удалось найти секцию");
                }

                if (section.Start == null)
                {
                    return Json($"Не удалось найти время начала секции");
                }

                if (section.Location == null)
                {
                    return Json($"Не удалось найти адрес секции");
                }
            }

            return Json(true);
        }

        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> CheckSectionExistance(int? sectionId, bool isSectionStartAndLocation)
        {
            if (isSectionStartAndLocation && sectionId == null)
            {
                return Json($"Выберите секцию");
            }

            var section = await _db.Sections.Where(s => s.Id == sectionId).SingleOrDefaultAsync();

            if (section == null)
            {
                return Json($"Не удалось найти секцию");
            }

            if (section.Start == null)
            {
                return Json($"Не удалось найти время начала секции");
            }

            if (section.Location == null)
            {
                return Json($"Не удалось найти адрес секции");
            }

            return Json(true);
        }
    }
}