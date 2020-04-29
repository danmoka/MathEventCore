using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MathEvent.Helpers;
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
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationContext _db;

        public ConferenceController(ApplicationContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var conferences = await _db.Conferences
                .Where(c => c.Start.Month >= DateTime.Now.Month)
                .Include(c => c.Manager)
                .Include(c => c.Sections)
                .ThenInclude(s => s.Performances).ToListAsync();

            var conferenceViewModels = new List<ConferenceViewModel>();

            foreach(var conference in conferences)
            {
                var conferenceViewModel = new ConferenceViewModel
                {
                    Name = conference.Name,
                    Location = conference.Location,
                    Start = conference.Start
                };

                var sectionViewModels = new List<SectionViewModel>();

                foreach(var section in conference.Sections)
                {
                    var sectionViewModel = new SectionViewModel
                    {
                        Name = section.Name,
                        Location = section.Location,
                        Start = section.Start
                    };

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
                            CreatorName = performance.Creator.Name,
                            DataPath = performance.DataPath,
                            PosterName = performance.PosterName,
                            Traffic = performance.Traffic,
                            Type = performance.Type
                        };

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
        public async Task<IActionResult> Add([Bind("Name", "Location", "Start", "End")] Conference conference)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Error400", "Error");
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Error500", "Error");
            }

            conference.ManagerId = user.Id;
            await _db.Conferences.AddAsync(conference);
            await _db.SaveChangesAsync();

            var conferenceDataPath = user.DataPath;
            if (!UserDataPathWorker.CreateSubDirectory(ref conferenceDataPath, conference.Id.ToString()))
            {
                _db.Conferences.Remove(conference);
                await _db.SaveChangesAsync();

                return RedirectToAction("Error500", "Error");
            }

            conference.DataPath = conferenceDataPath;
            _db.Conferences.Update(conference);
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
                return RedirectToAction("Error500", "Error");
            }

            if (conference.ManagerId != _userManager.GetUserId(User))
            {
                return RedirectToAction("Index", "Home");
            }

            return View(conference);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            [Bind("Id, Name", "Location", "Start", "End", "ManagerId")] Conference conference)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Error400", "Error");
            }

            _db.Conferences.Update(conference);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index", "Account");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Delete(int conferenceId)
        {
            var conference = await _db.Conferences.Where(p => p.Id == conferenceId).SingleOrDefaultAsync();

            if (conference == null)
            {
                return RedirectToAction("Error500", "Error");
            }

            if (conference.ManagerId != _userManager.GetUserId(User))
            {
                return RedirectToAction("Error500", "Error");
            }

            var path = UserDataPathWorker.GetRootPath(conference.DataPath);

            if (Directory.Exists(path))
            {
                try
                {
                    Directory.Delete(path, true);
                }
                catch
                {
                    return RedirectToAction("Error500", "Error");
                }

            }

            _db.Conferences.Remove(conference);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index", "Account");
        }

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