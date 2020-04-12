using System;
using System.Linq;
using System.Threading.Tasks;
using MathEvent.Helpers;
using MathEvent.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MathEvent.Controllers
{
    public class SectionController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationContext _db;

        public SectionController(ApplicationContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Add()
        {
            var user = await _userManager.GetUserAsync(User);
            var userConferences = await _db.Conferences.Where(c => c.ManagerId == user.Id).ToListAsync();

            if (userConferences.Count() == 0)
            {
                return RedirectToAction("Add", "Conference");
            }

            ViewBag.Conferences = new SelectList(userConferences, "Id", "Name");

            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([Bind("Name", "Location", "Start", "End", "ConferenceId")] Section section)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Error400", "Error");
            }

            var user = await _userManager.GetUserAsync(User);
            section.ManagerId = user.Id;
            await _db.Sections.AddAsync(section);
            await _db.SaveChangesAsync();

            var conference = await _db.Conferences.Where(c => c.Id == section.ConferenceId).SingleAsync();
            var sectionDataPath = conference.DataPath;
            
            if (!UserDataPathWorker.CreateSubDirectory(ref sectionDataPath, section.Id.ToString()))
            {
                _db.Sections.Remove(section);
                await _db.SaveChangesAsync();

                return RedirectToAction("Error500", "Error");
            }
            
            section.DataPath = sectionDataPath;
            await _db.SaveChangesAsync();

            return RedirectToAction("Add", "Section");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Edit(int sectionId)
        {
            var section = await _db.Sections.Where(c => c.Id == sectionId).SingleAsync();
            var user = await _userManager.GetUserAsync(User);
            var userConferences = await _db.Conferences.Where(c => c.ManagerId == user.Id).ToListAsync();

            if (userConferences.Count() == 0)
            {
                return RedirectToAction("Add", "Conference");
            }

            ViewBag.Conferences = new SelectList(userConferences, "Id", "Name");

            return View(section);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("Id", "Name", "Location", "Start", "End", "DataPath", 
            "ConferenceId", "ManagerId")] Section section)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Error400", "Error");
            }

            _db.Sections.Update(section);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index", "Account");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Delete(int sectionId)
        {
            var section = await _db.Sections.Where(p => p.Id == sectionId).SingleAsync();

            if (System.IO.File.Exists(section.DataPath))
            {
                System.IO.File.Delete(section.DataPath);
            }

            _db.Sections.Remove(section);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index", "Account");
        }

        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> CheckStartDate(DateTime start, DateTime end, int conferenceId)
        {
            var conference = await _db.Conferences.Where(c => c.Id == conferenceId).SingleAsync();

            if (start < conference.Start || start > conference.End)
            {
                return Json($"Дата {start} выходит за временные рамки конференции {conference.Name}.");
            }  
            else if (start < DateTime.Now)
            {
                return Json($"Дата {start} меньше текущей даты {DateTime.Now}.");
            }
            else if (start > end)
            {
                return Json($"Дата начала больше даты конца.");
            }

            return Json(true);
        }

        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> CheckEndDate(DateTime end, DateTime start, int conferenceId)
        {
            var conference = await _db.Conferences.Where(c => c.Id == conferenceId).SingleAsync();

            if (end < conference.Start || end > conference.End)
            {
                return Json($"Дата {end} выходит за временные рамки конференции {conference.Name}.");
            }
            else if (end < DateTime.Now)
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