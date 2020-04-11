using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MathEvent.Helpers;
using MathEvent.Models;
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

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Add()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(Conference conference)
        {
            // если conference == null, то создат ьи вернуть странциу BadRequest

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userManager.GetUserAsync(User);
            conference.ManagerId = user.Id;
            _db.Conferences.Add(conference);
            await _db.SaveChangesAsync();

            var conferenceDataPath = user.DataPath;
            UserDataPathWorker.CreateSubDirectory(ref conferenceDataPath, conference.Id.ToString());
            // todo: проверка, что папка создалась
            conference.DataPath = conferenceDataPath;
            await _db.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Edit(int conferenceId)
        {
            // если id null, то что?
            var conference = _db.Conferences.Where(c => c.Id == conferenceId).FirstOrDefault();
            // если не нашли, то что?

            return View(conference);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Conference conference)
        {
            _db.Conferences.Update(conference);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index", "Account");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int conferenceId)
        {
            var conference = await _db.Conferences.Where(p => p.Id == conferenceId).FirstAsync();
            _db.Conferences.Remove(conference);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index", "Account");
        }
    }
}