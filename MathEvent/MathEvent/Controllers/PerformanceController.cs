using System;
using System.Collections.Generic;
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
using Microsoft.AspNetCore.Routing;
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

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            IEnumerable<Performance> performances = await _db.Performances
                .Where(p => p.Start.Month >= DateTime.Now.Month)
                .Include(p => p.Section)
                .Include(p => p.Creator).ToListAsync();

            var cards = new List<PerformanceViewModel>();

            foreach (var performance in performances)
            {
                var card = new PerformanceViewModel
                {
                    Id = performance.Id,
                    Name = performance.Name,
                    Annotation = performance.Annotation,
                    KeyWords = performance.KeyWords,
                    Start = performance.Start,
                    CreatorName = $"{performance.Creator.Name} {performance.Creator.Surname}",
                    DataPath = performance.DataPath,
                    PosterName = performance.PosterName,
                    Traffic = performance.Traffic,
                    Type = performance.Type,
                    Location = performance.Location
                };

                cards.Add(card);
            }

            return View(cards);
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

            if (user == null)
            {
                return RedirectToAction("Error500", "Error");
            }

            var userSections = await _db.Sections.Where(s => s.ManagerId == user.Id).ToListAsync();
            ViewBag.Sections = new SelectList(userSections, "Id", "Name");

            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(
            [Bind("Name", "Type", "Location", "KeyWords", "Annotation", "Start", "SectionId")] Performance performance, IFormFile uploadedFile)
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

            performance.CreatorId = user.Id;
            await _db.Performances.AddAsync(performance);
            await _db.SaveChangesAsync();

            string performanceDataPath;

            if (performance.SectionId != null)
            {
                var section = await _db.Sections.Where(s => s.Id == performance.SectionId).SingleOrDefaultAsync();

                if (section == null)
                {
                    return RedirectToAction("Error500", "Error");
                }

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

            if (uploadedFile != null)
            {
                performance.PosterName = Path.GetFileName(uploadedFile.FileName);
            }
            else
            {
                performance.PosterName = Path.GetFileName(UserDataPathWorker.GetDefaultImagePath());
            }

            await UserDataPathWorker.UploadImage(uploadedFile, performance.DataPath, performance.PosterName);

            await _db.SaveChangesAsync();

            return RedirectToAction("Index", "Performance");
        }

        [HttpGet]
        public async Task<IActionResult> Card(int id)
        {
            var performance = await _db.Performances.Where(p => p.Id == id)
                .Include(p => p.Section)
                .Include(p => p.Creator).SingleOrDefaultAsync();

            if (performance == null)
            {
                return RedirectToAction("Error500", "Error");
            }

            var card = new PerformanceViewModel
            {
                Id = performance.Id,
                Name = performance.Name,
                Annotation = performance.Annotation,
                KeyWords = performance.KeyWords,
                Start = performance.Start,
                CreatorName = $"{performance.Creator.Name} {performance.Creator.Surname}",
                DataPath = performance.DataPath,
                PosterName = performance.PosterName,
                Traffic = performance.Traffic,
                Location = performance.Location,
                Info = performance.Creator.Info
            };

            if (_signInManager.IsSignedIn(User))
            {
                var user = await _userManager.GetUserAsync(User);
                var userId = user.Id;

                var ap = await _db.ApplicationUserPerformances.Where(ap => ap.PerformanceId == id && ap.ApplicationUserId == userId).SingleOrDefaultAsync();

                card.IsSignedUp = ap != null;
            }

            return View(card);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Edit(int performanceId)
        {
            var performance = await _db.Performances.Where(c => c.Id == performanceId).SingleOrDefaultAsync();

            if (performance == null)
            {
                return RedirectToAction("Error500", "Error");
            }

            if (performance.CreatorId != _userManager.GetUserId(User))
            {
                return RedirectToAction("Index", "Home");
            }

            var types = DataFactory.GetPerformanceTypes().GetValues()
                 .Select(x => new SelectListItem { Text = x, Value = x })
                 .ToList();
            ViewBag.Types = types;
            var user = await _userManager.GetUserAsync(User);
            var userSections = await _db.Sections.Where(s => s.ManagerId == user.Id).ToListAsync();
            ViewBag.Sections = new SelectList(userSections, "Id", "Name");

            return View(performance);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("Id", "Name", "Location", "Type", "KeyWords", "Annotation", "Start", "SectionId",
            "CreatorId", "DataPath", "PosterName", "Traffic")] Performance performance, IFormFile uploadedFile, IFormFile uploadedProceedings)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Error400", "Error");
            }

            if (uploadedFile != null)
            {
                var imageToBeDeleted = UserDataPathWorker.GetRootPath(Path.Combine(performance.DataPath, performance.PosterName));

                if (System.IO.File.Exists(imageToBeDeleted))
                {
                    System.IO.File.Delete(imageToBeDeleted);
                }

                performance.PosterName = Path.GetFileName(uploadedFile.FileName);
                await UserDataPathWorker.UploadImage(uploadedFile, performance.DataPath, performance.PosterName);
            }

            if (uploadedProceedings != null)
            {
                performance.ProceedingsName = Path.GetFileName(uploadedProceedings.FileName);
                await UserDataPathWorker.UploadFile(uploadedProceedings, performance.DataPath, performance.ProceedingsName);
            }

            _db.Performances.Update(performance);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index", "Account");
        }

        [HttpGet]
        public IActionResult About()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Delete(int performanceId)
        {
            //todo: возможность удалять админу
            var performance = await _db.Performances.Where(p => p.Id == performanceId).SingleOrDefaultAsync();

            if (performance == null)
            {
                return RedirectToAction("Error500", "Error");
            }

            if (performance.CreatorId != _userManager.GetUserId(User))
            {
                return RedirectToAction("Error500", "Error");
            }

            var path = UserDataPathWorker.GetRootPath(performance.DataPath);

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
            
            _db.Performances.Remove(performance);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index", "Account");
        }

        public async Task<IActionResult> DownloadProceedings(int performanceId)
        {
            var performance = await _db.Performances.Where(p => p.Id == performanceId).SingleOrDefaultAsync();

            if (performance != null)
            {
                var filePath = UserDataPathWorker.GetRootPath(UserDataPathWorker.ConcatPaths(
                    performance.DataPath, performance.ProceedingsName));
                byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);

                return File(fileBytes, "application/pdf", performance.ProceedingsName);
            }

            return BadRequest();
        }

        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> CheckStartDate(DateTime start, int? sectionId)
        {
            if (start < DateTime.Now)
            {
                return Json($"Дата {start} меньше текущей даты {DateTime.Now}");
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

        [AcceptVerbs("Get", "Post")]
        public IActionResult CheckType(string type)
        {
            if (!DataFactory.GetPerformanceTypes().GetValues().Contains(type))
            {
                return Json($"Тип {type} не существует");
            }

            return Json(true);
        }

        private async Task UnsubscribeUsers(int performanceId)
        {
            var applicationUserPerformances = await _db.ApplicationUserPerformances.Where(ap => ap.PerformanceId == performanceId).ToListAsync();

            foreach(var ap in applicationUserPerformances)
            {
                _db.ApplicationUserPerformances.Remove(ap);
            }

            await _db.SaveChangesAsync();
        }
    }
}