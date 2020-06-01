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
        
        [HttpGet]
        public IActionResult Index()
        {
            return View();
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
            var userConferences = new List<Conference>();

            if (User.IsInRole("admin"))
            {
                // переделать section add view на blazor и брать конференции из DbService
                userConferences = await _db.Conferences.ToListAsync();
            }
            else
            {
                userConferences = await _db.Conferences.Where(c => c.ManagerId == user.Id).ToListAsync();
            }

            if (userConferences == null)
            {
                return RedirectToAction("Error404", "Error");
            }

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
        public async Task<IActionResult> Add(
            [Bind("Name", "Location", "Start", "End", "ConferenceId")] Section model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Error404", "Error");
            }

            model.ManagerId = user.Id;
            await _db.Sections.AddAsync(model);
            await _db.SaveChangesAsync();

            var conference = await _db.Conferences.Where(c => c.Id == model.ConferenceId).SingleOrDefaultAsync();

            if (conference == null)
            {
                return RedirectToAction("Error404", "Error");
            }

            var sectionDataPath = conference.DataPath;
            
            if (!UserDataPathWorker.CreateSubDirectory(ref sectionDataPath, model.Id.ToString()))
            {
                _db.Sections.Remove(model);
                await _db.SaveChangesAsync();

                return RedirectToAction("Error500", "Error");
            }
            
            model.DataPath = sectionDataPath;
            await _db.SaveChangesAsync();

            return RedirectToAction("Index", "Conference");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Edit(int sectionId)
        {
           
            var section = await _db.Sections.Where(c => c.Id == sectionId).SingleOrDefaultAsync();

            if (section == null)
            {
                return RedirectToAction("Error404", "Error");
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Error404", "Error");
            }

            if (!await IsSectionModifier(sectionId))
            {
                return RedirectToAction("Error403", "Error");
            }

            var sectionModel = new SectionViewModel
            {
                Id = section.Id,
                Name = section.Name,
                Start = section.Start,
                End = section.End,
                Location = section.Location,
                ConferenceId = section.ConferenceId,
                DataPath = section.DataPath,
                UserId = user.Id
            };

            return View(sectionModel);
        }

        //[HttpPost]
        //[Authorize]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(
        //    [Bind("Id", "Name", "Location", "Start", "End", "DataPath", "ConferenceId", "ManagerId")] Section section)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return RedirectToAction("Error400", "Error");
        //    }

        //    var dbSection = await _db.Sections.Where(s => s.Id == section.Id).SingleOrDefaultAsync();

        //    if (dbSection == null)
        //    {
        //        return RedirectToAction("Error500", "Error");
        //    }

        //    if (!await IsSectionModifier(section.Id))
        //    {
        //        return RedirectToAction("Error500", "Error");
        //    }

        //    dbSection.Name = section.Name;
        //    dbSection.Location = section.Location;
        //    dbSection.Start = section.Start;
        //    dbSection.End = section.End;

        //    _db.Sections.Update(dbSection);
        //    await _db.SaveChangesAsync();

        //    return RedirectToAction("Index", "Account");
        //}

        //[HttpGet]
        //[Authorize]
        //public async Task<IActionResult> Delete(int sectionId)
        //{
        //    var section = await _db.Sections.Where(p => p.Id == sectionId).SingleOrDefaultAsync();

        //    if (section == null)
        //    {
        //        return RedirectToAction("Error500", "Error");
        //    }

        //    var user = await _userManager.GetUserAsync(User);

        //    if (user == null)
        //    {
        //        return RedirectToAction("Error500", "Error");
        //    }

        //    if (! await IsSectionModifier(sectionId))
        //    {
        //        return RedirectToAction("Error500", "Error");
        //    }

        //    var path = UserDataPathWorker.GetRootPath(section.DataPath);

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

        //    _db.Sections.Remove(section);
        //    await _db.SaveChangesAsync();

        //    return RedirectToAction("Index", "Account");
        //}

        [HttpGet]
        public IActionResult About()
        {
            return View();
        }

        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> CheckStartDate(DateTime start, DateTime end, int conferenceId)
        {
            var conference = await _db.Conferences.Where(c => c.Id == conferenceId).SingleAsync();

            if (start < conference.Start || start > conference.End)
            {
                return Json($"Дата {start} выходит за временные рамки конференции {conference.Name}: {conference.Start} - {conference.End}.");
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
                return Json($"Дата {end} выходит за временные рамки конференции {conference.Name}: {conference.Start} - {conference.End}.");
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

        private async Task<bool> IsSectionModifier(int sectionId)
        {
            var section = await _db.Sections.Where(s => s.Id == sectionId)
                .Include(c => c.Conference)
                .SingleOrDefaultAsync();

            var isModifier = false;

            if (section == null)
            {
                return isModifier;
            }

            var user = await _userManager.GetUserAsync(User);

            if (section.ManagerId == user.Id)
            {
                isModifier |= true;
            }

            if (section.Conference != null &&
                section.Conference.ManagerId == user.Id)
            {
                isModifier |= true;
            }

            if (User.IsInRole("admin"))
            {
                isModifier |= true;
            }

            return isModifier;
        }
    }
}