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
    /// <summary>
    /// Контроллер действий с конференциями
    /// </summary>
    public class ConferenceController : Controller
    {
        private readonly ApplicationContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly UserService _userService;

        public ConferenceController(ApplicationContext db, UserManager<ApplicationUser> userManager, UserService userService)
        {
            _db = db;
            _userManager = userManager;
            _userService = userService;
        }

        /// <summary>
        /// Предоставляет страницу с коллекцией будующих конференций
        /// </summary>
        /// <returns>Страница с коллекцией конференций</returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var conferences = await _db.Conferences
                .Where(c => c.Start.Month >= DateTime.Now.Month)
                .Include(c => c.Manager)
                .Include(c => c.Sections)
                .ThenInclude(s => s.Performances)
                .ThenInclude(p => p.Creator).ToListAsync();

            if (conferences == null)
            {
                return RedirectToAction("Error404", "Error");
            }

            var conferenceViewModels = new List<ConferenceViewModel>();

            foreach(var conference in conferences)
            {
                var conferenceViewModel = new ConferenceViewModel
                {
                    Id = conference.Id,
                    Name = conference.Name,
                    Location = conference.Location,
                    Start = conference.Start
                };

                if (conference.Sections == null)
                {
                    return RedirectToAction("Error404", "Error");
                }

                var sectionViewModels = new List<SectionViewModel>();

                foreach(var section in conference.Sections)
                {
                    var sectionViewModel = new SectionViewModel
                    {
                        Name = section.Name,
                        Location = section.Location,
                        Start = section.Start
                    };

                    if (section.Performances == null)
                    {
                        return RedirectToAction("Error404", "Error");
                    }

                    var performanceViewModels = new List<PerformanceViewModel>();

                    foreach(var performance in section.Performances)
                    {
                        var performanceViewModel = new PerformanceViewModel
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
                            Location = performance.Location
                        };

                        if (performance.Creator != null)
                        {
                            performanceViewModel.CreatorName = performance.Creator.Name;
                        }

                        performanceViewModels.Add(performanceViewModel);
                    }

                    sectionViewModel.PerformanceViewModels = performanceViewModels;
                    sectionViewModels.Add(sectionViewModel);
                }

                conferenceViewModel.SectionViewModels = sectionViewModels;
                conferenceViewModels.Add(conferenceViewModel);
            }

            return View(conferenceViewModels);
        }

        [HttpGet]
        [Authorize]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(
            [Bind("Name", "Location", "Start", "End")] Conference model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Error500", "Error");
            }

            model.ManagerId = user.Id;
            await _db.Conferences.AddAsync(model);
            await _db.SaveChangesAsync();

            /// после создания конференции нужно добавить папку на сервер
            var conferenceDataPath = user.DataPath;

            if (!UserDataPathWorker.CreateSubDirectory(ref conferenceDataPath, model.Id.ToString()))
            {
                _db.Conferences.Remove(model);
                await _db.SaveChangesAsync();

                return RedirectToAction("Error500", "Error");
            }

            model.DataPath = conferenceDataPath;
            _db.Conferences.Update(model);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index", "Conference");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Edit(int conferenceId)
        {
            var conference = await _db.Conferences
                .Where(c => c.Id == conferenceId)
                .SingleOrDefaultAsync();

            if (conference == null)
            {
                return RedirectToAction("Error404", "Error");
            }

            var user = await _userManager.GetUserAsync(User);

            /// юзер авторизован, но мы не можем его найти
            if (user == null)
            {
                return RedirectToAction("Error404", "Error");
            }

            /// если юзер каким-либо обзразом попадает на страницу изменения конференции и не является легитимным редактором
            if (!await _userService.IsConferenceModifier(conference.Id, user.Id))
            {
                return RedirectToAction("Error403", "Error");
            }

            var conferenceModel = new ConferenceViewModel
            {
                Id = conference.Id,
                Name = conference.Name,
                Location = conference.Location,
                Start = conference.Start,
                End = conference.End,
                UserId = user.Id,
            };

            return View(conferenceModel);
        }

        [HttpGet]
        public IActionResult About()
        {
            return View();
        }

        /// <summary>
        /// Метод валидации даты начала
        /// </summary>
        /// <param name="start">Дата начала</param>
        /// <param name="end">Дата конца</param>
        /// <returns>Json(true), если данные валидны, иначе сообщение об ошибке</returns>
        [AcceptVerbs("Get", "Post")]
        public IActionResult CheckStartDate(DateTime start, DateTime end)
        {
            if (start < DateTime.Now)
            {
                return Json($"Дата {start} меньше текущей даты {DateTime.Now}");
            }
            else if (start > end)
            {
                return Json($"Дата начала больше даты конца");
            }

            return Json(true);
        }

        /// <summary>
        /// Метод валидации даты конца
        /// </summary>
        /// <param name="end">Дата конца</param>
        /// <param name="start">Дата начала</param>
        /// <returns>Json(true), если данные валидны, иначе сообщение об ошибке</returns>
        [AcceptVerbs("Get", "Post")]
        public IActionResult CheckEndDate(DateTime end, DateTime start)
        {
            if (end < DateTime.Now)
            {
                return Json($"Дата {end} меньше текущей даты {DateTime.Now}");
            }
            else if (start > end)
            {
                return Json($"Дата начала больше даты конца");
            }

            return Json(true);
        }
    }
}