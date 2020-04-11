using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MathEvent.Helpers;
using MathEvent.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MathEvent.Controllers
{
    public class PerformanceController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationContext _db;
        private static IWebHostEnvironment _webHostEnvironment;

        public PerformanceController(ApplicationContext db, UserManager<ApplicationUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var performances = await _db.Performances
                .Include(p => p.Section)
                .Include(p => p.Creator).ToListAsync();
            //foreach(var performance in performances)
            //{
            //    performance.DataPath = Path.Combine(_webHostEnvironment.WebRootPath, performance.DataPath);
            //    performance.PosterName = Path.Combine(performance.DataPath, performance.PosterName);
            //}
            return View(performances);
        }

        [HttpGet]
        public async Task<IActionResult> Add()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var types = DataFactory.GetPerformanceTypes().GetValues()
                 .Select(x => new SelectListItem { Text = x, Value = x })
                 .ToList();
            ViewBag.Types = types;
            var user = await _userManager.GetUserAsync(User);
            var userSections = await _db.Sections.Where(s => s.ManagerId == user.Id).ToListAsync();
            ViewBag.Sections = new SelectList(userSections, "Id", "Name");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(Performance performance, IFormFile uploadedFile)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userManager.GetUserAsync(User);
            performance.CreatorId = user.Id;
            await _db.Performances.AddAsync(performance);
            await _db.SaveChangesAsync();

            string performanceDataPath;

            if (performance.SectionId != null)
            {
                var section = await _db.Sections.Where(s => s.Id == performance.SectionId).FirstOrDefaultAsync();
                performanceDataPath = section.DataPath;
            }
            else
            {
                performanceDataPath = user.DataPath;
            }

            UserDataPathWorker.CreateSubDirectory(ref performanceDataPath, performance.Id.ToString());
            performance.DataPath = performanceDataPath;

            if (uploadedFile != null)
            {
                performance.PosterName = Path.GetFileName(uploadedFile.FileName);
                using var fileStream = new FileStream(UserDataPathWorker.GetRootPath(Path.Combine(performance.DataPath, performance.PosterName)), FileMode.Create);
                await uploadedFile.CopyToAsync(fileStream);
                //fileUpload.SaveAs(Path.Combine(performance.DataPath, performance.PosterName));
            }
            else
            {
                performance.PosterName = Path.GetFileName(UserDataPathWorker.GetDefaultImagePath());
                using var deafaultImg = new FileStream(UserDataPathWorker.GetRootPath(UserDataPathWorker.GetDefaultImagePath()), FileMode.Open);
                using var fileStream = new FileStream(UserDataPathWorker.GetRootPath(Path.Combine(performance.DataPath, performance.PosterName)), FileMode.Create);
                await deafaultImg.CopyToAsync(fileStream);
            }

            await _db.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Card(int? performanceId)
        {
            if (performanceId == null)
            {
                // сделать HttpNotFound
                return RedirectToAction("Index", "Home");
            }

            var user = await _userManager.GetUserAsync(User);
            var userId = user.Id;

            var ap = await _db.ApplicationUserPerformances.Where(ap => ap.PerformanceId == performanceId && ap.ApplicationUserId == userId).FirstOrDefaultAsync();

            if (ap == null)
            {
                ViewBag.SignedUp = false;
            }
            else
            {
                ViewBag.SignedUp = true;
            }

            var performance = await _db.Performances.Where(p => p.Id == performanceId)
                .Include(p => p.Section)
                .Include(p => p.Creator).FirstAsync();

            return View(performance);
        }

        [HttpGet]
        public async Task<IActionResult> Subscribe(int? performanceId)
        {
            if (performanceId == null)
            {
                // сделать HttpNotFound
                return RedirectToAction("Index", "Home");
            }

            var user = await _userManager.GetUserAsync(User);
            var userId = user.Id;

            var ap = await _db.ApplicationUserPerformances.Where(ap => ap.PerformanceId == performanceId && ap.ApplicationUserId == userId).FirstOrDefaultAsync();

            if (ap == null)
            {
                ap = new ApplicationUserPerformance()
                {
                    ApplicationUserId = user.Id,
                    PerformanceId = (int) performanceId
                };

                await _db.ApplicationUserPerformances.AddAsync(ap);
                await _db.SaveChangesAsync();
            }
            // если уже записан, то вывести что-нибудь

            return RedirectToAction("Card", "Performance", new { performanceId = performanceId});
        }

        [HttpGet]
        public async Task<IActionResult> UnSubscribe(int? performanceId)
        {
            if (performanceId == null)
            {
                // сделать HttpNotFound
                return RedirectToAction("Index", "Home");
            }

            var user = await _userManager.GetUserAsync(User);
            var userId = user.Id;

            var ap = await _db.ApplicationUserPerformances.Where(ap => ap.PerformanceId == performanceId && ap.ApplicationUserId == userId).FirstOrDefaultAsync();

            if (ap != null)
            {
                _db.ApplicationUserPerformances.Remove(ap);
                await _db.SaveChangesAsync();
            }
            // если еще не записан, то вывести что-нибудь

            return RedirectToAction("Card", "Performance", new { performanceId = performanceId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int performanceId)
        {
            // если id null, то что?
            var performance = await _db.Performances.Where(c => c.Id == performanceId).FirstAsync();
            // если не нашли, то что?

            return View(performance);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Performance performance)
        {
            _db.Performances.Update(performance);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index", "Account");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int performanceId)
        {
            var performance = await _db.Performances.Where(p => p.Id == performanceId).FirstAsync();
            _db.Performances.Remove(performance);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index", "Account");
        }
    }
}