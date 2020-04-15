using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MathEvent.Helpers;
using MathEvent.Models;
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

        public async Task<IActionResult> Index()
        {
            var conferences = await _db.Conferences
                .Include(c => c.Manager)
                .Include(c => c.Sections)
                .ThenInclude(s => s.Performances).ToListAsync();

            return View(conferences);
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
            var conference = await _db.Conferences.Where(c => c.Id == conferenceId).SingleAsync();

            return View(conference);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("Id, Name", "Location", "Start", "End", "ManagerId")] Conference conference)
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
            var conference = await _db.Conferences.Where(p => p.Id == conferenceId).SingleAsync();

            if (System.IO.File.Exists(conference.DataPath))
            {
                System.IO.File.Delete(conference.DataPath);
            }

            _db.Conferences.Remove(conference);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index", "Account");
        }

        [AcceptVerbs("Get", "Post")]
        public IActionResult CheckStartDate(DateTime start, DateTime end)
        {
            if (start < DateTime.Now)
            {
                return Json($"Дата {start} меньше текущей даты {DateTime.Now}.");
            }
            else if (start > end)
            {
                return Json($"Дата начала больше даты конца.");
            }

            return Json(true);
        }

        [AcceptVerbs("Get", "Post")]
        public IActionResult CheckEndDate(DateTime end, DateTime start)
        {
            if (end < DateTime.Now)
            {
                return Json($"Дата {end} меньше текущей даты {DateTime.Now}.");
            }
            else if (start > end)
            {
                return Json($"Дата начала больше даты конца.");
            }

            return Json(true);
        }
    }
}