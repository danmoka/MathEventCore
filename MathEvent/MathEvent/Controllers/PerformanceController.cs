using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MathEvent.Helpers;
using MathEvent.Models;
using MathEvent.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
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
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationContext _db;
        private static IWebHostEnvironment _webHostEnvironment;

        public PerformanceController(ApplicationContext db, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _userManager = userManager;
            _signInManager = signInManager;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var performances = await _db.Performances
                .Include(p => p.Section)
                .Include(p => p.Creator).ToListAsync();

            return View(performances);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Add()
        {
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
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([Bind("Name", "Type", "KeyWords", "Annotation", "Start", "SectionId")] Performance performance, IFormFile uploadedFile)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Error400", "Error");
            }

            var user = await _userManager.GetUserAsync(User);
            performance.CreatorId = user.Id;
            await _db.Performances.AddAsync(performance);
            await _db.SaveChangesAsync();

            string performanceDataPath;

            if (performance.SectionId != null)
            {
                var section = await _db.Sections.Where(s => s.Id == performance.SectionId).SingleAsync();
                performanceDataPath = section.DataPath;
            }
            else
            {
                performanceDataPath = user.DataPath;
            }

            if(!UserDataPathWorker.CreateSubDirectory(ref performanceDataPath, performance.Id.ToString()))
            {
                _db.Performances.Remove(performance);
                await _db.SaveChangesAsync();

                return RedirectToAction("Error500", "Error");
            }

            performance.DataPath = performanceDataPath;

            // а что если записать файл не получится?
            if (uploadedFile != null)
            {
                performance.PosterName = Path.GetFileName(uploadedFile.FileName);
                using var fileStream = new FileStream(UserDataPathWorker.GetRootPath(Path.Combine(performance.DataPath, performance.PosterName)), FileMode.Create);
                await uploadedFile.CopyToAsync(fileStream);
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
        public async Task<IActionResult> Card(int performanceId)
        {
            var performance = await _db.Performances.Where(p => p.Id == performanceId)
                .Include(p => p.Section)
                .Include(p => p.Creator).SingleAsync();

            var card = new CardViewModel
            {
                Id = performance.Id,
                Name = performance.Name,
                Annotation = performance.Annotation,
                KeyWords = performance.KeyWords,
                Start = performance.Start,
                CreatorId = performance.CreatorId,
                CreatorName = performance.Creator.Name,
                DataPath = performance.DataPath,
                PosterName = performance.PosterName,
                Traffic = performance.Traffic
            };

            if (_signInManager.IsSignedIn(User))
            {
                var user = await _userManager.GetUserAsync(User);
                var userId = user.Id;

                var ap = await _db.ApplicationUserPerformances.Where(ap => ap.PerformanceId == performanceId && ap.ApplicationUserId == userId).SingleOrDefaultAsync();

                card.IsSignedUp = ap != null;
            }

            return View(card);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Subscribe(int performanceId)
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user.Id;
            var ap = await _db.ApplicationUserPerformances.Where(ap => ap.PerformanceId == performanceId && ap.ApplicationUserId == userId).SingleOrDefaultAsync();

            if (ap == null)
            {
                ap = new ApplicationUserPerformance()
                {
                    ApplicationUserId = user.Id,
                    PerformanceId = performanceId
                };

                await _db.ApplicationUserPerformances.AddAsync(ap);
                var performance = await _db.Performances.Where(p => p.Id == performanceId).SingleAsync(); // или лучше триггер?
                performance.Traffic++;
                _db.Performances.Update(performance);
                await _db.SaveChangesAsync();
            }
            else
            {
                _db.ApplicationUserPerformances.Remove(ap);
                var performance = await _db.Performances.Where(p => p.Id == performanceId).SingleAsync(); // или лучше триггер?
                performance.Traffic--; // если меньше 0 получается, то это ошибка проектирования (наверное) где-то, в бд стоит check(>= 0). Но будет он отрабатывать - неизвестно. Но это не его проблемы, решать эту проблему надо тут.
                // в модели стоит Range, должен отработать скрипт, что значение меньше 0
                _db.Performances.Update(performance);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction("Index", "Performance", new { performanceId = performanceId});
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Edit(int performanceId)
        {
            var performance = await _db.Performances.Where(c => c.Id == performanceId).SingleAsync();
            var user = await _userManager.GetUserAsync(User);
            var userSections = await _db.Sections.Where(s => s.ManagerId == user.Id).ToListAsync();
            ViewBag.Sections = new SelectList(userSections, "Id", "Name");

            return View(performance);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("Id", "Name", "Type", "KeyWords", "Annotation", "Start", "SectionId",
            "CreatorId", "DataPath", "PosterName", "Traffic")] Performance performance, IFormFile uploadedFile)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Error400", "Error");
            }

            // что если картинку не удалось загрузить?
            if (uploadedFile != null)
            {
                var imageToBeDeleted = UserDataPathWorker.GetRootPath(Path.Combine(performance.DataPath, performance.PosterName));

                if (System.IO.File.Exists(imageToBeDeleted))
                {
                    System.IO.File.Delete(imageToBeDeleted);
                }

                performance.PosterName = Path.GetFileName(uploadedFile.FileName);
                using var fileStream = new FileStream(UserDataPathWorker.GetRootPath(Path.Combine(performance.DataPath, performance.PosterName)), FileMode.Create);
                await uploadedFile.CopyToAsync(fileStream);
            }

            _db.Performances.Update(performance);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index", "Account");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Delete(int performanceId)
        {
            var performance = await _db.Performances.Where(p => p.Id == performanceId).SingleAsync();

            if (System.IO.File.Exists(performance.DataPath))
            {
                System.IO.File.Delete(performance.DataPath);
            }
            
            _db.Performances.Remove(performance);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index", "Account");
        }

        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> CheckStartDate(DateTime start, int? sectionId)
        {
            if (start < DateTime.Now)
            {
                return Json($"Дата {start} меньше текущей даты {DateTime.Now}.");
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
    }
}