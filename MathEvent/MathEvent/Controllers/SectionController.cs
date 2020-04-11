using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MathEvent.Helpers;
using MathEvent.Models;
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
        public async Task<IActionResult> Add()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userManager.GetUserAsync(User);
            var userConferences = _db.Conferences.Where(c => c.ManagerId == user.Id);

            if (userConferences.Count() == 0)
            {
                return RedirectToAction("Add", "Conference");
            }

            ViewBag.Conferences = new SelectList(userConferences, "Id", "Name");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(Section section)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            
            var user = await _userManager.GetUserAsync(User);
            section.ManagerId = user.Id;

            _db.Sections.Add(section);
            await _db.SaveChangesAsync();

            var conference = _db.Conferences.Where(c => c.Id == section.ConferenceId).FirstOrDefault();
            // проверить даты секции и конференции

            var sectionDataPath = conference.DataPath;
            UserDataPathWorker.CreateSubDirectory(ref sectionDataPath, section.Id.ToString());
            // todo: проверка, что папка создалась
            section.DataPath = sectionDataPath;
            await _db.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Edit(int sectionId)
        {
            // если id null, то что?
            var section = _db.Sections.Where(c => c.Id == sectionId).FirstOrDefault();
            // если не нашли, то что?

            return View(section);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Section section)
        {
            _db.Sections.Update(section);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index", "Account");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int sectionId)
        {
            var section = await _db.Sections.Where(p => p.Id == sectionId).FirstAsync();
            _db.Sections.Remove(section);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index", "Account");
        }
    }
}