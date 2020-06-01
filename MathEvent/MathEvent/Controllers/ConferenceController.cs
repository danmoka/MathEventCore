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
        public async Task<IActionResult> Add([Bind("Name", "Location", "Start", "End")] Conference model)
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
            var conference = await _db.Conferences.Where(c => c.Id == conferenceId).SingleOrDefaultAsync();

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

        //[HttpPost]
        //[Authorize]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(
        //    [Bind("Id, Name", "Location", "Start", "End", "ManagerId")] Conference model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(model);
        //    }

        //    var conference = await _db.Conferences.Where(c => c.Id == model.Id).SingleOrDefaultAsync();

        //    /// не удается найти конференцию по id
        //    if (conference == null)
        //    {
        //        return RedirectToAction("Error500", "Error");
        //    }

        //    /// если юзер каким-либо обзразом отправляет пост запрос изменения конференции и не является легитимным редактором
        //    if (!await IsConferenceModifier(conference.Id))
        //    {
        //        return RedirectToAction("Error403", "Error");
        //    }

        //    conference.Name = model.Name;
        //    conference.Location = model.Location;
        //    conference.Start = model.Start;
        //    conference.End = model.End;

        //    _db.Conferences.Update(conference);
        //    await _db.SaveChangesAsync();

        //    return RedirectToAction("Index", "Account");
        //}

        //[HttpGet]
        //[Authorize]
        //public async Task<IActionResult> Delete(int conferenceId)
        //{
        //    var conference = await _db.Conferences.Where(p => p.Id == conferenceId).SingleOrDefaultAsync();

        //    /// не удается найти конференцию по id
        //    if (conference == null)
        //    {
        //        return RedirectToAction("Error500", "Error");
        //    }

        //    /// если юзер каким-либо обзразом отправляет гет запрос удаления конференции и не является легитимным редактором
        //    if (! await IsConferenceModifier(conferenceId))
        //    {
        //        return RedirectToAction("Error500", "Error");
        //    }

        //    var path = UserDataPathWorker.GetRootPath(conference.DataPath);

        //    if (Directory.Exists(path))
        //    {
        //        try
        //        {
        //            Directory.Delete(path, true);
        //        }
        //        catch
        //        {
        //            return RedirectToAction("Error500", "Error");
        //        }

        //    }

        //    _db.Conferences.Remove(conference);
        //    await _db.SaveChangesAsync();

        //    return RedirectToAction("Index", "Account");
        //}

        [HttpGet]
        public IActionResult About()
        {
            return View();
        }

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